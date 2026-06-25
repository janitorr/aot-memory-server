using AotMemoryServer.Data;
using Microsoft.EntityFrameworkCore;

namespace AotMemoryServer.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/api/health", async (AppDbContext db) =>
        {
            var ok = await db.Database.CanConnectAsync();
            return ok ? Results.Ok(new { status = "healthy" }) : Results.StatusCode(503);
        });

        app.MapGet("/api/ready", async (AppDbContext db) =>
        {
            var canConnect = await db.Database.CanConnectAsync();
            var applied = await db.Database.GetAppliedMigrationsAsync();
            var ready = canConnect && applied.Any();
            return ready ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503);
        });
    }
}
