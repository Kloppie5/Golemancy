using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MemoryObserver;

public class APIController
{
    HttpListener _listener;
    Dictionary<string, Func<HttpListenerContext, object>> commands = new();

    public APIController(int port)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{port}/");
        _listener.Start();
        Console.WriteLine($"Started listening to HTTP requests on port {port}");


    }

    public void RegisterCommandHander(string command, Func<HttpListenerContext, object> handler)
    {
        commands.Add(command, handler);
    }

    public void Listen()
    {
        while (true)
        {
            var ctx = _listener.GetContext();
            var req = ctx.Request;
            var res = ctx.Response;

            try
            {
                string command = req.Url.AbsolutePath;
                if (command == "/favicon.ico")
                    continue;

                Console.WriteLine($"Received command '{req.Url.AbsolutePath}'");

                if (commands.TryGetValue(command, out var func))
                {
                    var response = func(ctx);
                    WriteJson(res, response);
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
