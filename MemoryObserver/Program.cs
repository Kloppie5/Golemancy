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

        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:5000/");
        listener.Start();
        Console.WriteLine("Started listening to HTTP requests on port 5000");

        while (true)
        {
            var ctx = listener.GetContext();
            var req = ctx.Request;
            var res = ctx.Response;

            try
            {
                if (req.Url.AbsolutePath == "/favicon.ico")
                {
                    continue;
                }

                Console.WriteLine($"Got something on '{req.Url.AbsolutePath}'");
                // Console.WriteLine(req.QueryString);

                if (req.Url.AbsolutePath == "/regions")
                {
                    var regions = pm.ListRegions();
                    WriteJson(res, regions);
                }
            }
            catch (Exception ex)
            {
                res.StatusCode = 500;
                var buf = System.Text.Encoding.UTF8.GetBytes(ex.ToString());
                res.OutputStream.Write(buf, 0, buf.Length);
            }
            res.OutputStream.Close();
        }
    }
    static void WriteJson(HttpListenerResponse res, object obj)
    {
        string json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        var buf = System.Text.Encoding.UTF8.GetBytes(json);
        res.ContentType = "application/json";
        res.OutputStream.Write(buf, 0, buf.Length);
    }
}
