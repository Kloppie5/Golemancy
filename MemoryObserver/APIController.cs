using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MemoryObserver;

public class MonoBridge
{
    private readonly ProcessManager _pm;
    private readonly int _rootDomain;

    public MonoBridge(ProcessManager pm, int rootDomain)
    {
        _pm = pm;
        _rootDomain = rootDomain;
    }

    public void MapEndpoints(WebApplication app)
    {
        // Assembly -> Image
        app.MapGet("/mono/assembly/{name}", (string name) =>
        {
            int asm = ProcessManager.MonoDomainGetMonoAssemblyByName(_rootDomain, name);
            int img = ProcessManager.MonoAssemblyGetMonoImage(asm);
            return Results.Json(new { assembly = name, asm, img });
        });

        // Class lookup
        app.MapGet("/mono/class/{img}/{ns}/{cls}", (int img, string ns, string cls) =>
        {
            int clsPtr = ProcessManager.MonoImageGetMonoClassByName(img, ns, cls);
            return Results.Json(new { ns, cls, ptr = clsPtr });
        });

        // VTable for a class
        app.MapGet("/mono/vtable/{cls}", (int cls) =>
        {
            int vtable = ProcessManager.MonoClassGetMonoVTable(_rootDomain, cls);
            return Results.Json(new { cls, vtable });
        });

        // Static field data
        app.MapGet("/mono/vtable/{vtable}/staticfields", (int vtable) =>
        {
            int sf = ProcessManager.VTableGetStaticFieldData(vtable);
            return Results.Json(new { vtable, staticFieldData = sf });
        });

        // Read object/string/array
        app.MapGet("/mono/read/int/{addr}", (int addr) =>
        {
            return Results.Json(new { addr, value = _pm.ReadUnsafe<int>(addr) });
        });

        app.MapGet("/mono/read/string/{addr}", (int addr) =>
        {
            string s = _pm.ReadUnsafeMonoWideString(addr);
            return Results.Json(new { addr, value = s });
        });

        app.MapGet("/mono/read/array/{addr}", (int addr) =>
        {
            int len = _pm.ReadUnsafe<int>(addr + 0xC);
            return Results.Json(new { addr, length = len });
        });

        // Dump dictionary
        app.MapGet("/mono/read/dict/{addr}", (int addr) =>
        {
            var dict = _pm.ReadDictionaryTypeObject(addr);
            return Results.Json(dict);
        });
    }
}

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
