using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        var mem = new MemoryService();
        var input = new InputService();

        app.MapGet("/status", () => Results.Ok(new { ok = true }));
        app.MapPost("/click", async (MouseActionRequest r) =>
        {
            // r.screenX, r.screenY, r.button, r.delayMs
            input.Click(r.screenX, r.screenY, r.button, r.delayMs);
            return Results.Ok();
        });
        app.MapPost("/drag", async (DragRequest r) =>
        {
            input.Drag(r.fromX, r.fromY, r.toX, r.toY, r.durationMs);
            return Results.Ok();
        });
        app.MapGet("/readPointer", (ulong baseAddr, int bytes) =>
        {
            var data = mem.ReadBytes((nint)baseAddr, bytes);
            return Results.Ok(Convert.ToBase64String(data));
        });
        app.MapGet("/scanPattern", (string patternHex) =>
        {
            var hits = mem.ScanPattern(patternHex);
            return Results.Ok(hits);
        });

        app.Run("http://localhost:5000");
    }
}

// --- DTOs
public record MouseActionRequest(int screenX, int screenY, string button = "left", int delayMs = 10);
public record DragRequest(int fromX, int fromY, int toX, int toY, int durationMs = 200);