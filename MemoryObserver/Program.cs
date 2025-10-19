using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Mono.Cecil;
using Golemancy.Services;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        var input = new InputService();

        app.MapGet("/status", () => Results.Ok(new { ok = true }));

        app.MapPost("/click", async (MouseActionRequest r) =>
        {
            input.Click(r.screenX, r.screenY, r.button, r.delayMs);
            return Results.Ok();
        });

        app.MapPost("/drag", async (DragRequest r) =>
        {
            input.Drag(r.fromX, r.fromY, r.toX, r.toY, r.durationMs);
            return Results.Ok();
        });

        app.MapGet("/mono/classes", (string assemblyPath) =>
        {
            if (!File.Exists(assemblyPath))
                return Results.NotFound($"Assembly not found: {assemblyPath}");

            var classes = MonoInspector.GetClasses(assemblyPath);
            return Results.Ok(classes);
        });

        app.MapGet("/mono/classinfo", (string assemblyPath, string className) =>
        {
            if (!File.Exists(assemblyPath))
                return Results.NotFound($"Assembly not found: {assemblyPath}");

            var info = MonoInspector.GetClassInfo(assemblyPath, className);
            return info is not null ? Results.Ok(info) : Results.NotFound($"Class not found: {className}");
        });

        app.MapGet("/mono/dump", (string assemblyPath) =>
        {
            if (!File.Exists(assemblyPath))
                return Results.NotFound($"Assembly not found: {assemblyPath}");

            var dump = MonoInspector.DumpAssembly(assemblyPath);
            return Results.Ok(dump);
        });

        app.MapGet("/memory/read", (string processName, long address, int size) =>
        {
            try
            {
                var data = MemoryReader.ReadFromProcess(processName, new IntPtr(address), size);
                if (data == null || data.Length == 0)
                    return Results.NoContent();

                return Results.File(data, "application/octet-stream");
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        app.Run("http://localhost:5000");
    }
}

// --- DTOs
public record MouseActionRequest(int screenX, int screenY, string button = "left", int delayMs = 10);
public record DragRequest(int fromX, int fromY, int toX, int toY, int durationMs = 200);