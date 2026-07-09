using Mittens.Application.Serialization;
using Mittens.Data;
using Microsoft.EntityFrameworkCore;

namespace Mittens.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/api/health", async (AppDbContext db) =>
        {
            try
            {
                if (!await db.Database.CanConnectAsync())
                    return Results.StatusCode(503);

                await db.Database.ExecuteSqlRawAsync("SELECT 1");
                return Results.Ok(new HealthStatus("healthy"));
            }
            catch
            {
                return Results.StatusCode(503);
            }
        });
    }
}
