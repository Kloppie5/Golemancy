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
    public static extern Int32 GetModuleFileNameEx( nint hProcess, nint hModule, [Out] StringBuilder lpBaseName, int nSize );

    private const uint PROCESS_ALL_ACCESS = 0x001F0FFF;

    Process? _process;
    nint _hProcess;

    public ProcessManager( string name )
    {
        _process = Process.GetProcessesByName(name).FirstOrDefault();
        if (_process is null)
            throw new ArgumentException($"Could not find process '{name}'");
        _hProcess = (nint)OpenProcess(PROCESS_ALL_ACCESS, false, _process.Id);
    }



    public nint GetModule64ByNameOrThrow( string name ) {
        nint[] hModules = new nint[1024];
        int lpcbNeeded = 0;
        EnumProcessModulesEx(_hProcess, hModules, 1024, ref lpcbNeeded, 0x3);
        for ( int i = 0; i < lpcbNeeded / IntPtr.Size; i++ ) {
            StringBuilder sb = new(1024);
            GetModuleFileNameEx(_hProcess, hModules[i], sb, 1024);
            if ( sb.ToString().Contains(name) )
                return hModules[i];
        }
        throw new ArgumentException($"Could not find module '{name}'");
    }

    public static T? Read<T> ( Process process, nint address ) where T : struct {
        int size = Marshal.SizeOf(typeof(T));
        byte[] read = new byte[size];

        ReadProcessMemory(process.Handle, address, read, size, out int _);

        T? result;
        GCHandle handle = GCHandle.Alloc(read, GCHandleType.Pinned);

        try {
            result = (T?) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        } finally {
            handle.Free();
        }

        return result;
    }
}