using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Golemancy;

public partial class ProcessManager
{
    [DllImport("kernel32.dll")]
    static extern long CreateRemoteThread (long hProcess, long lpThreadAttributes, uint dwStackSize, long lpStartAddress, long lpParameter, uint dwCreationFlags, long lpThreadId );
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern long OpenProcess( uint dwDesiredAccess, bool bInheritHandle, int dwProcessId );
    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory( long hProcess, long lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead );
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    public static extern long VirtualAllocEx ( long hProcess, long lpAddress, long dwSize, uint  flAllocationType, uint flProtect );
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern bool VirtualFreeEx (long hProcess, long lpAddress, long dwSize, uint dwFreeType );
    [DllImport("kernel32.dll")]
    public static extern bool WriteProcessMemory( long hProcess, long lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten );
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint WaitForSingleObject (long hHandle, uint dwMilliseconds );

    [DllImport("psapi.dll")]
    public static extern bool EnumProcessModulesEx ( long hProcess, [Out] long[] lphModule, int cb, ref int lpcbNeeded, int dwFilterFlag );
    [DllImport("psapi.dll")]
    public static extern int GetModuleFileNameEx( long hProcess, long hModule, [Out] StringBuilder lpBaseName, int nSize );

    private const uint PROCESS_ALL_ACCESS = 0x001F0FFF;

    Process? _process;
    long _hProcess;
    Dictionary<long, Dictionary<string, long>> _exportedFunctions = [];

    protected long _monoModule;
    protected int _monoRootDomain;

    public ProcessManager( )
    {
        string processName = "cultistsimulator";
        _process = Process.GetProcessesByName(processName).FirstOrDefault();
        if (_process is null)
            throw new ArgumentException($"Could not find process '{processName}'");
        _hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, _process.Id);

        _monoModule = GetModule64ByNameOrThrow("mono-2.0-bdwgc.dll");
        long getRootDomain = GetExportedFunctionByNameOrThrow(_monoModule, "mono_get_root_domain");
        _monoRootDomain = CallFunction<int>(getRootDomain);
    }

#region PE

    public long GetModule64ByNameOrThrow( string name )
    {
        long[] hModules = new long[1024];
        int lpcbNeeded = 0;
        EnumProcessModulesEx(_hProcess, hModules, 1024, ref lpcbNeeded, 0x3);
        for ( int i = 0; i < lpcbNeeded / IntPtr.Size; i++ )
        {
            StringBuilder sb = new(1024);
            GetModuleFileNameEx(_hProcess, hModules[i], sb, 1024);
            if ( sb.ToString().Contains(name) )
                return hModules[i];
        }
        throw new ArgumentException($"Could not find module '{name}'");
    }

    public Dictionary<string, long> GetExportedFunctions( long module )
    {
        var dosHeader = Read<IMAGE_DOS_HEADER>(module)
            ?? throw new ArgumentException($"Could not find dos header in module '{module}'");
        var optionalHeader32 = Read<IMAGE_OPTIONAL_HEADER32>(module + dosHeader.e_lfanew + 0x18)
            ?? throw new ArgumentException($"Could not find optional header 32 in module '{module}'");
        var exportTable = Read<IMAGE_EXPORT_DIRECTORY_TABLE>(module + optionalHeader32.ExportTable.VirtualAddress)
            ?? throw new ArgumentException($"Could not find export table in module '{module}'");
        
        Dictionary<string, long> exportedFunctions = [];

        for (int i = 0; i < exportTable.AddressTableEntries; ++i )
        {
            int FunctionNameOffset = ReadUnsafe<int>(module + exportTable.NamePointerRva + i * 4);
            string FunctionName = ReadUnsafeUTF8String(module + FunctionNameOffset);
            int FunctionOffset = ReadUnsafe<int>(module + exportTable.ExportAddressTableRva + i * 4);
            exportedFunctions.Add(FunctionName, module + FunctionOffset);
        }

        return exportedFunctions;
    }
    public long GetExportedFunctionByNameOrThrow( long module, string name )
    {
        if (!_exportedFunctions.ContainsKey(module))
            _exportedFunctions[module] = GetExportedFunctions(module);
        return _exportedFunctions[module][name];
    }

#endregion
#region Mono

    public T CallFunction<T> ( long address ) where T : struct
    {
        long dataspace = VirtualAllocEx(_hProcess, 0, 0x1000, 0x3000, 0x40);

        byte[] code = AssemblyBuilder.Start()
            .MOVi32r(0, (int)address)
            .CALLr(0)
            .MOVr0m32((int)dataspace)
            .RET()
            .Finalize();

        long codespace = VirtualAllocEx(_hProcess, 0, code.Length, 0x3000, 0x40);
        WriteProcessMemory(_hProcess, codespace, code, code.Length, out int _);
        long thread = CreateRemoteThread(_hProcess, 0, 0, codespace, 0, 0, 0);
        WaitForSingleObject(thread, 0xFFFFFFFF);
        VirtualFreeEx(_hProcess, codespace, code.Length, 0x8000);
        return ReadUnsafe<T>(dataspace);
    }
    public T CallMonoFunctionUnsafe<T> (string functionName, params object[] args ) where T : struct
        => CallMonoFunction<T>(functionName, args).Value;
    public T? CallMonoFunction<T> (string functionName, params object[] args ) where T : struct
    {
        int functionAddress = (int)GetExportedFunctionByNameOrThrow(_monoModule, functionName);
        int mono_thread_attach = (int)GetExportedFunctionByNameOrThrow(_monoModule, "mono_thread_attach");

        AssemblyBuilder assembler = AssemblyBuilder.Start();

        if ( functionName != "mono_get_root_domain" ) {
            assembler
                .PUSHi32(_monoRootDomain)
                .MOVi32r(0, mono_thread_attach)
                .CALLr(0)
                .ADDi8r(4, 4);
        }


        assembler.MOVi32r(0, functionAddress);
        for ( int i = args.Length - 1; i >= 0; --i )
        {
            switch (args[i])
            {
                case int v:
                    assembler.PUSHi32(v);
                    break;
                case string v1:
                    int stringAddress = (int)VirtualAllocEx(_hProcess, 0, 0x1000, 0x3000, 0x40);
                    WriteProcessMemory(_hProcess, stringAddress, Encoding.UTF8.GetBytes(v1), v1.Length, out int _);
                    assembler.PUSHi32(stringAddress);
                    break;
                default:
                    throw new Exception($"Unsupported argument type {args[i].GetType()}");
            }
        }
        assembler.CALLr(0);

        int dataSpace = (int)VirtualAllocEx(_hProcess, 0, 0x1000, 0x3000, 0x40);
        assembler.MOVr0m32(dataSpace);
        assembler.ADDi8r(4, (sbyte) (4 * args.Length));


        assembler.RET();
        byte[] code = assembler.Finalize();
        
        long codespace = VirtualAllocEx(_hProcess, 0, code.Length, 0x3000, 0x40);
        WriteProcessMemory(_hProcess, codespace, code, code.Length, out int _);
        long thread = CreateRemoteThread(_hProcess, 0, 0, codespace, 0, 0, 0);
        WaitForSingleObject(thread, 0xFFFFFFFF);
        VirtualFreeEx(_hProcess, codespace, code.Length, 0x8000);

        return Read<T>(dataSpace);
    }

#endregion
#region Memory Management

    public Dictionary<string, int> ReadDictionaryTypeObject (int pointer ) {
        Dictionary<string, int> dict = [];
        int entries = ReadUnsafe<int>(pointer + 0xC);
        int count = ReadUnsafe<int>(entries + 0xC);

        for (long i = 0; i < count; ++i ) {
            int hashcode = ReadUnsafe<int>(entries + 0x10 + i * 0x10);
            int next = ReadUnsafe<int>(entries + 0x14 + i * 0x10);
            int key = ReadUnsafe<int>(entries + 0x18 + i * 0x10);
            int value = ReadUnsafe<int>(entries + 0x1C + i * 0x10);
            if ( value == 0 )
                continue;
            int keyType = ReadUnsafe<int>(key + 0x8);
            int keyClass = ReadUnsafe<int>(keyType);
            string keyClassName = ReadUnsafeUTF8String(ReadUnsafe<int>(keyClass + 0x2C));
            dict.Add(keyClassName, value);
        }

        return dict;
    }

    public T? Read<T>( long address ) where T : struct
    {
        int size = Marshal.SizeOf(typeof(T));
        byte[] read = new byte[size];

        ReadProcessMemory(_hProcess, address, read, size, out int _);

        T? result;
        GCHandle handle = GCHandle.Alloc(read, GCHandleType.Pinned);

        try
        {
            result = (T?) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        }
        finally
        {
            handle.Free();
        }

        return result;
    }
    
    public T ReadUnsafe<T>( long address ) where T : struct
    {
        int size = Marshal.SizeOf(typeof(T));
        byte[] read = new byte[size];

        ReadProcessMemory(_hProcess, address, read, size, out int _);

        T? result;
        GCHandle handle = GCHandle.Alloc(read, GCHandleType.Pinned);

        try
        {
            result = (T?) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        }
        finally
        {
            handle.Free();
        }

        if (result is null)
            throw new Exception($"Unable to read {typeof(T).Name} from {address}");

        return result.Value;
    }
    public string ReadUnsafeUTF8String( long address )
    {
        List<byte> bytes = [];

        byte b;
        while ( (b = ReadUnsafe<byte>(address++)) != 0 )
            bytes.Add(b);

        return Encoding.UTF8.GetString(bytes.ToArray());
    }
    public string ReadUnsafeMonoWideString ( long address) {
        List<byte> bytes = [];

        int length = ReadUnsafe<int>(address + 0x8);
        for ( int i = 0; i < length; i++ )
            bytes.Add(ReadUnsafe<byte>(address + 0xC + i*2));

        return Encoding.UTF8.GetString(bytes.ToArray());
    }

#endregion
}