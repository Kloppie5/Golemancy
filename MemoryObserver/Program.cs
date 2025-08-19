using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MemoryObserver;

public class Program
{
    static ProcessManager _pm;

    public static void Main(string[] args)
    {
        Process[] processes = Process.GetProcessesByName("cultistsimulator");
        _pm = new ProcessManager(processes[0]);

        var api = new APIController(5000);

        api.RegisterCommandHander("/read", ReadHandler);
        api.RegisterCommandHander("/regions", RegionsHandler);

        api.Listen();
    }

    static object ReadHandler(HttpListenerContext ctx)
    {
        var query = ctx.Request.QueryString;
        string addrParam = query["addr"]
            ?? throw new ArgumentNullException("addr");
        string lenParam = query["len"]
            ?? throw new ArgumentNullException("len");

        long addr = Convert.ToInt64(addrParam, 16);
        int len = int.Parse(lenParam);

        var data = _pm.ReadRegion(addr, len);
        return new
        {
            addr = addr,
            len = len,
            data = data
        };
    }

    static List<object> RegionsHandler(HttpListenerContext ctx)
    {
        return _pm.ListRegions();
    }
}
