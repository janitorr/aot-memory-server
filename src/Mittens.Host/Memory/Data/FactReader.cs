using System.Data;
using System.Data.Common;
using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Mittens.Core.Fact;
using Mittens.Core.Shared;

namespace Mittens.Host.Memory.Data;

public class FactReader(AppDbContext db) : IFactReader
{
    public async Task<Fact?> GetByIdAsync(int id, CancellationToken ct)
    {
        await using var cmd = CreateCommand();
        cmd.CommandText = FactSql.GetById;
        cmd.Parameters.Add(new SqliteParameter("@p0", id));

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
            return ReadFact(reader);
        return null;
    }

    public async Task<Fact?> GetByCategoryKeyScopeAsync(string category, string key, string scope, CancellationToken ct)
    {
        await using var cmd = CreateCommand();
        cmd.CommandText = FactSql.GetByCategoryKeyScope;
        cmd.Parameters.Add(new SqliteParameter("@p0", category));
        cmd.Parameters.Add(new SqliteParameter("@p1", key));
        cmd.Parameters.Add(new SqliteParameter("@p2", scope));

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
            return ReadFact(reader);
        return null;
    }

    public async Task<PagedResult<Fact>> ListAsync(string? category, string? scope, string? key, int page, int pageSize, CancellationToken ct)
    {
        var offset = (page - 1) * pageSize;

        var totalCount = await ExecuteCountAsync(FactSql.GetFactsCount, ct,
            ("@p0", category), ("@p1", scope), ("@p2", key));
        var items = await ReadFactsAsync(FactSql.GetFactsPage, ct,
            ("@p0", category), ("@p1", scope), ("@p2", key), ("@p3", pageSize), ("@p4", offset));

        return new PagedResult<Fact>(items, totalCount, page, pageSize);
    }

    public async Task<PagedResult<Fact>> SearchAsync(string q, string? category, string? scope, int page, int pageSize, CancellationToken ct)
    {
        var offset = (page - 1) * pageSize;

        var totalCount = await ExecuteCountAsync(FactSql.SearchFactsCount, ct,
            ("@p0", q), ("@p1", category), ("@p2", scope));
        var items = await ReadFactsAsync(FactSql.SearchFactsPage, ct,
            ("@p0", q), ("@p1", category), ("@p2", scope), ("@p3", pageSize), ("@p4", offset));

        return new PagedResult<Fact>(items, totalCount, page, pageSize);
    }

    private async Task<int> ExecuteCountAsync(string sql, CancellationToken ct, params (string Name, object? Value)[] parameters)
    {
        await using var cmd = CreateCommand();
        cmd.CommandText = sql;
        foreach (var (name, value) in parameters)
            cmd.Parameters.Add(new SqliteParameter(name, value ?? DBNull.Value));

        var result = await cmd.ExecuteScalarAsync(ct);
        return result is long l ? (int)l : Convert.ToInt32(result, CultureInfo.InvariantCulture);
    }

    private async Task<List<Fact>> ReadFactsAsync(string sql, CancellationToken ct, params (string Name, object? Value)[] parameters)
    {
        await using var cmd = CreateCommand();
        cmd.CommandText = sql;
        foreach (var (name, value) in parameters)
            cmd.Parameters.Add(new SqliteParameter(name, value ?? DBNull.Value));

        var items = new List<Fact>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            items.Add(ReadFact(reader));
        return items;
    }

    private SqliteCommand CreateCommand()
    {
        var conn = (SqliteConnection)db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            conn.Open();
        return conn.CreateCommand();
    }

    private static Fact ReadFact(DbDataReader reader)
    {
        return new Fact
        {
            Id = reader.GetInt32(0),
            Category = reader.GetString(1),
            Key = reader.GetString(2),
            Value = reader.GetString(3),
            Scope = reader.GetString(4),
            Confidence = reader.GetDouble(5),
            Source = reader.IsDBNull(6) ? null : reader.GetString(6),
            UpdatedAt = DateTimeOffset.Parse(reader.GetString(7), CultureInfo.InvariantCulture),
            IsDeprecated = reader.GetBoolean(8),
        };
    }
}
