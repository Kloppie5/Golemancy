
using System.Diagnostics;

namespace Golemancy;

class Program {
    public static void Main ( ) {
        Console.WriteLine("Knock knock Mansus");

        var pm = new ProcessManager("cultistsimulator");
        nint module = pm.GetModule64ByNameOrThrow("mono-2.0-bdwgc.dll");

        Console.WriteLine($"Found module {module}");

    }
}