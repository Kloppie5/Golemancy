using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace MemoryObserver;

public class ProcessManager
{
    [DllImport("kernel32.dll")]
    static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public IntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }

    const int PROCESS_VM_READ = 0x0010;
    const int PROCESS_QUERY_INFORMATION = 0x0400;

    private IntPtr handle;
    public ProcessManager(Process process)
    {
        Console.WriteLine($"Initializing ProcessManager for process {process.ProcessName} ({process.Id})");
        handle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, process.Id);
        if (handle == IntPtr.Zero)
            throw new Exception("OpenProcess failed");
    }

    public List<object> ListRegions()
    {
        var result = new List<object>();
        IntPtr addr = IntPtr.Zero;
        MEMORY_BASIC_INFORMATION mbi;

        while (VirtualQueryEx(handle, addr, out mbi, (uint)Marshal.SizeOf<MEMORY_BASIC_INFORMATION>()) != 0)
        {
            long size = mbi.RegionSize.ToInt64();
            if (mbi.State == 0x1000) // MEM_COMMIT
            {
                result.Add(new
                {
                    baseAddr = mbi.BaseAddress.ToInt64(),
                    size = size,
                    protect = mbi.Protect
                });
            }
            addr = (IntPtr)(mbi.BaseAddress.ToInt64() + size);
        }
        return result;
    }
}