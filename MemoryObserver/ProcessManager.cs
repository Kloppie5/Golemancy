using System.Diagnostics;

namespace MemoryObserver;

public class ProcessManager
{
    public ProcessManager(Process process)
    {
        Console.WriteLine($"Initializing ProcessManager for process {process.ProcessName} ({process.Id})");
    }
}