using System.Data;
using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Mittens.Core.Fact;

namespace Mittens.Host.Memory.Data;

public class FactWriter(AppDbContext db) : IFactWriter
{
    public async Task<Fact> InsertAsync(Fact fact, CancellationToken ct)
    {
        fact.UpdatedAt = DateTimeOffset.UtcNow;

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "MittensFacts" ("Category", "Key", "Value", "Scope", "Confidence", "Source", "UpdatedAt", "IsDeprecated")
            VALUES ({fact.Category}, {fact.Key}, {fact.Value}, {fact.Scope}, {fact.Confidence}, {fact.Source}, {fact.UpdatedAt}, {fact.IsDeprecated})
            """, ct);

        var conn = (SqliteConnection)db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            conn.Open();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT last_insert_rowid()";
        var id = await cmd.ExecuteScalarAsync(ct);
        fact.Id = id is long l ? (int)l : Convert.ToInt32(id, CultureInfo.InvariantCulture);
        return fact;
    }

    public async Task UpdateAsync(Fact fact, int id, CancellationToken ct)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE "MittensFacts" SET "Category" = {fact.Category}, "Key" = {fact.Key}, "Value" = {fact.Value}, "Scope" = {fact.Scope}, "Confidence" = {fact.Confidence}, "Source" = {fact.Source}, "UpdatedAt" = {fact.UpdatedAt}, "IsDeprecated" = {fact.IsDeprecated} WHERE "Id" = {id}
            """, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            DELETE FROM "MittensFacts" WHERE "Id" = {id}
            """, ct);
    }
}
