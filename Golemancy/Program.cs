
using System.Diagnostics;

namespace Golemancy;

class Program {
    public static void Main ( ) {
        Console.WriteLine("Knock knock Mansus");

        var process = Process.GetProcessesByName("cultistsimulator").FirstOrDefault();
        if (process is null)
        {
            Console.WriteLine("[X] Could not find Process");
            return;
        }
        Console.WriteLine($"Found process {process.Id}");
        
        nint? module = ProcessManager.GetModules64ByName(process.Handle, "mono-2.0-bdwgc.dll").FirstOrDefault();
        if (module is null)
        {
            Console.WriteLine("[X] Could not find Module");
        }
        Console.WriteLine($"Found module {module}");
    }
}