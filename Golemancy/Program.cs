
using System.Diagnostics;

namespace Golemancy;

class Program {
    public static void Main ( ) {
        Console.WriteLine("Knock knock Mansus");

        var process = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == "cultistsimulator");
        if (process is null)
        {
            Console.WriteLine("[X] Could not find Process");
            return;
        }
        Console.WriteLine($"Found process {process.Id}");
    }
}