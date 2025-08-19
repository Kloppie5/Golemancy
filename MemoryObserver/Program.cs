using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MemoryObserver;

public class Program
{
    public static void Main(string[] args)
    {
        Process[] processes = Process.GetProcessesByName("cultistsimulator");
        var pm = new ProcessManager(processes[0]);

        var api = new APIController(5000);

        api.RegisterCommandHander("/regions", (ctx) => pm.ListRegions());

        api.Listen();
    }
}
