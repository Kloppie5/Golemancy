using System;
using System.Collections.Concurrent;
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

    static long _commandCount = 0;
    static Stopwatch _uptime = Stopwatch.StartNew();
    static object _lock = new object();
    
    static ConcurrentDictionary<string, long> _commandTypes = new ConcurrentDictionary<string, long>();

    public static void Main(string[] args)
    {
        Process[] processes = Process.GetProcessesByName("cultistsimulator");
        _pm = new ProcessManager(processes[0]);

        var api = new APIController(5000);

        api.RegisterCommandHander("/hexdump", HexDumpHandler);
        api.RegisterCommandHander("/read", ReadHandler);
        api.RegisterCommandHander("/regions", RegionsHandler);

        api.Listen();
    }

    static object HexDumpHandler(HttpListenerContext ctx)
    {
        var query = ctx.Request.QueryString;
        string addrParam = query["addr"]
            ?? throw new ArgumentNullException("addr");
        string lenParam = query["len"]
            ?? throw new ArgumentNullException("len");

        long addr = Convert.ToInt64(addrParam, 16);
        int len = int.Parse(lenParam);

        var rawdata = _pm.ReadRegion(addr, len);

        var sb = new StringBuilder();
        var data = new List<string>();
        for (int i = 0; i < rawdata.Length; i += 16)
        {
            sb.Append($"{addr + i:X8}:  ");
            for (int j = 0; j < 16; j++)
            {
                if (i + j < rawdata.Length)
                    sb.Append($"{rawdata[i + j]:X2} ");
                else
                    sb.Append("   ");
            }
            sb.Append(" | ");
            for (int j = 0; j < 16 && i + j < rawdata.Length; j++)
            {
                byte b = rawdata[i + j];
                sb.Append(b >= 32 && b <= 126 ? (char)b : '.');
            }
            data.Add(sb.ToString());
            sb.Clear();
        }
        return new
        {
            addr = addr,
            len = len,
            data = data
        };
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
