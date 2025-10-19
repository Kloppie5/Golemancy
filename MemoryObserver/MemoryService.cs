using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class MemoryService
{
    private Process _proc;
    private IntPtr _hProcess = IntPtr.Zero;
    private const int PROCESS_VM_READ = 0x0010;
    private const int PROCESS_QUERY_INFORMATION = 0x0400;
    private const int PROCESS_VM_OPERATION = 0x0008;
    private const int PROCESS_VM_WRITE = 0x0020;
    public MemoryService()
    {
        AttachToProcess("CultistSimulator"); // try common process name
    }
    public bool AttachToProcess(string name)
    {
        var procs = Process.GetProcessesByName(name);
        if (procs.Length == 0) return false;
        _proc = procs[0];
        _hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ | PROCESS_VM_OPERATION | PROCESS_VM_WRITE, false, _proc.Id);
        return _hProcess != IntPtr.Zero;
    }

    public byte[] ReadBytes(IntPtr addr, int size)
    {
        var buf = new byte[size];
        if (!ReadProcessMemory(_hProcess, addr, buf, size, out IntPtr bytesRead))
            throw new Exception("ReadProcessMemory failed: " + Marshal.GetLastWin32Error());
        return buf;
    }

    // Very simple hex pattern scanner across main module (example)
    public List<ulong> ScanPattern(string patternHex)
    {
        var results = new List<ulong>();
        var module = _proc.MainModule;
        var baseAddr = (ulong)module.BaseAddress;
        var moduleBytes = ReadBytes(module.BaseAddress, module.ModuleMemorySize);
        var pattern = HexStringToBytes(patternHex);
        for (int i = 0; i + pattern.Length < moduleBytes.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (pattern[j] == 0xFF) continue; // use 0xFF as wildcard if desired
                if (moduleBytes[i + j] != pattern[j]) { match = false; break; }
            }
            if (match) results.Add(baseAddr + (ulong)i);
        }
        return results;
    }

    private static byte[] HexStringToBytes(string s)
    {
        s = s.Replace(" ", "");
        var bytes = new List<byte>();
        for (int i = 0; i < s.Length; i += 2) bytes.Add(Convert.ToByte(s.Substring(i, 2), 16));
        return bytes.ToArray();
    }

    #region PInvoke
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);
    #endregion
}
