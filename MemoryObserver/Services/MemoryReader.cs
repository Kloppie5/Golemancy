using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Golemancy.Services
{
    public static class MemoryReader
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead);

        public static byte[]? ReadMemory(IntPtr processHandle, IntPtr address, int size)
        {
            var buffer = new byte[size];
            if (!ReadProcessMemory(processHandle, address, buffer, size, out int bytesRead))
            {
                int error = Marshal.GetLastWin32Error();

                // Win32 error 299: "Only part of a ReadProcessMemory or WriteProcessMemory request was completed."
                if (error == 299 && bytesRead > 0)
                    return buffer[..bytesRead];

                throw new Win32Exception(error);
            }
            return buffer[..bytesRead];
        }

        public static byte[]? ReadFromProcess(string processName, IntPtr address, int size)
        {
            var proc = Process.GetProcessesByName(processName).FirstOrDefault();
            if (proc == null)
                throw new InvalidOperationException($"Process '{processName}' not found.");

            return ReadMemory(proc.Handle, address, size);
        }
    }
}
