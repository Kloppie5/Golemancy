using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Golemancy;

class ProcessManager {
    
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern long OpenProcess( uint dwDesiredAccess, bool bInheritHandle, int dwProcessId );
    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory( nint hProcess, nint lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead );
    [DllImport("kernel32.dll")]
    public static extern bool WriteProcessMemory( nint hProcess, nint lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten );
    
    [DllImport("psapi.dll")]
    public static extern bool EnumProcessModulesEx ( nint hProcess, [Out] nint[] lphModule, int cb, ref int lpcbNeeded, int dwFilterFlag );
    [DllImport("psapi.dll")]
    public static extern int GetModuleFileNameEx( nint hProcess, nint hModule, [Out] StringBuilder lpBaseName, int nSize );

    private const uint PROCESS_ALL_ACCESS = 0x001F0FFF;

    Process? _process;
    nint _hProcess;
    Dictionary<nint, Dictionary<string, nint>> _exportedFunctions = [];

    public ProcessManager( string name )
    {
        _process = Process.GetProcessesByName(name).FirstOrDefault();
        if (_process is null)
            throw new ArgumentException($"Could not find process '{name}'");
        _hProcess = (nint)OpenProcess(PROCESS_ALL_ACCESS, false, _process.Id);
    }

    public nint GetModule64ByNameOrThrow( string name )
    {
        nint[] hModules = new nint[1024];
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

    public Dictionary<string, nint> GetExportedFunctions( nint module )
    {
        var dosHeader = Read<IMAGE_DOS_HEADER>(module)
            ?? throw new ArgumentException($"Could not find dos header in module '{module}'");
        var optionalHeader32 = Read<IMAGE_OPTIONAL_HEADER32>(module + dosHeader.e_lfanew + 0x18)
            ?? throw new ArgumentException($"Could not find optional header 32 in module '{module}'");
        var exportTable = Read<IMAGE_EXPORT_DIRECTORY_TABLE>(module + optionalHeader32.ExportTable.VirtualAddress)
            ?? throw new ArgumentException($"Could not find export table in module '{module}'");
        
        Dictionary<string, nint> exportedFunctions = [];

        for (int i = 0; i < exportTable.AddressTableEntries; ++i ) {
            int FunctionNameOffset = ReadUnsafe<int>(module + exportTable.NamePointerRva + i * 4);
            string FunctionName = ReadUnsafeUTF8String(module + FunctionNameOffset);
            int FunctionOffset = ReadUnsafe<int>(module + exportTable.ExportAddressTableRva + i * 4);
            exportedFunctions.Add(FunctionName, module + FunctionOffset);
        }

        return exportedFunctions;
    }
    public nint GetExportedFunctionByNameOrThrow( nint module, string name )
    {
        if (!_exportedFunctions.ContainsKey(module))
            _exportedFunctions[module] = GetExportedFunctions(module);
        return _exportedFunctions[module][name];
    }

    public T? Read<T>( long address ) where T : struct
        => Read<T>((nint)address);
    public T? Read<T>( nint address ) where T : struct
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
        => ReadUnsafe<T>((nint)address);
    public T ReadUnsafe<T>( nint address ) where T : struct
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
    public string ReadUnsafeUTF8String( nint address )
    {
        List<byte> bytes = [];

        byte b;
        while ( (b = ReadUnsafe<byte>(address++)) != 0 )
            bytes.Add(b);

        return Encoding.UTF8.GetString(bytes.ToArray());
    }
}