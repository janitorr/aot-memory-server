using Microsoft.EntityFrameworkCore;
using AotMemoryServer.Models;

namespace AotMemoryServer.Data;

public static class FactWriter
{
    public static async Task<MemoryFact> InsertAsync(AppDbContext db, MemoryFact fact, CancellationToken ct)
    {
        fact.UpdatedAt = DateTimeOffset.UtcNow;

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "MemoryFacts" ("Category", "Key", "Value", "Scope", "Confidence", "Source", "UpdatedAt", "IsDeprecated")
            VALUES ({fact.Category}, {fact.Key}, {fact.Value}, {fact.Scope}, {fact.Confidence}, {fact.Source}, {fact.UpdatedAt}, {fact.IsDeprecated})
            """, ct);

        var id = await db.Database.SqlQueryRaw<int>("SELECT last_insert_rowid() AS \"Value\"")
            .FirstOrDefaultAsync(ct);

        fact.Id = id;
        return fact;
    }

    public static async Task UpdateAsync(AppDbContext db, MemoryFact fact, int id, CancellationToken ct)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE "MemoryFacts" SET "Category" = {fact.Category}, "Key" = {fact.Key}, "Value" = {fact.Value}, "Scope" = {fact.Scope}, "Confidence" = {fact.Confidence}, "Source" = {fact.Source}, "UpdatedAt" = {fact.UpdatedAt}, "IsDeprecated" = {fact.IsDeprecated} WHERE "Id" = {id}
            """, ct);
    }

    public static async Task DeleteAsync(AppDbContext db, int id, CancellationToken ct)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            DELETE FROM "MemoryFacts" WHERE "Id" = {id}
            """, ct);
    }
}
