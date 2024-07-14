
using System.Diagnostics;

namespace Golemancy;

class Program {
    public static void Main ( )
    {
        Console.WriteLine("Knock knock Mansus");

        var pm = new ProcessManager("cultistsimulator");
        nint module = pm.GetModule64ByNameOrThrow("mono-2.0-bdwgc.dll");
        
        var getRootDomain = pm.GetExportedFunctionByNameOrThrow(module, "mono_get_root_domain");
        Console.WriteLine($"Found mono_get_root_domain {getRootDomain}");
    }
}