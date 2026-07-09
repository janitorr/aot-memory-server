using Microsoft.EntityFrameworkCore;
using AotMemoryServer.Models;
using AotMemoryServer.Application.Abstractions;

namespace AotMemoryServer.Data;

public static class FactReader
{
    public static async Task<MemoryFact?> GetByIdAsync(AppDbContext db, int id, CancellationToken ct)
    {
        return await db.MemoryFacts.FromSqlRaw(MemoryFactSql.GetById, id)
            .FirstOrDefaultAsync(ct);
    }

    public static async Task<MemoryFact?> GetByCategoryKeyScopeAsync(AppDbContext db, string category, string key, string scope, CancellationToken ct)
    {
        return await db.MemoryFacts.FromSqlRaw(MemoryFactSql.GetByCategoryKeyScope, category, key, scope)
            .FirstOrDefaultAsync(ct);
    }

    public static async Task<PagedResult<MemoryFact>> ListAsync(AppDbContext db, string? category, string? scope, string? key, int page, int pageSize, CancellationToken ct)
    {
        var offset = (page - 1) * pageSize;

        var totalCount = await db.Database.SqlQueryRaw<int>(MemoryFactSql.GetFactsCount,
            category ?? (object)DBNull.Value,
            scope ?? (object)DBNull.Value,
            key ?? (object)DBNull.Value)
            .FirstOrDefaultAsync(ct);

        var items = await db.MemoryFacts.FromSqlRaw(MemoryFactSql.GetFactsPage,
            category ?? (object)DBNull.Value,
            scope ?? (object)DBNull.Value,
            key ?? (object)DBNull.Value,
            pageSize,
            offset)
            .ToListAsync(ct);

        return new PagedResult<MemoryFact>(items, totalCount, page, pageSize);
    }

    public static async Task<PagedResult<MemoryFact>> SearchAsync(AppDbContext db, string q, string? category, string? scope, int page, int pageSize, CancellationToken ct)
    {
        var offset = (page - 1) * pageSize;

        var totalCount = await db.Database.SqlQueryRaw<int>(MemoryFactSql.SearchFactsCount,
            q,
            category ?? (object)DBNull.Value,
            scope ?? (object)DBNull.Value)
            .FirstOrDefaultAsync(ct);

        var items = await db.MemoryFacts.FromSqlRaw(MemoryFactSql.SearchFactsPage,
            q,
            category ?? (object)DBNull.Value,
            scope ?? (object)DBNull.Value,
            pageSize,
            offset)
            .ToListAsync(ct);

        return new PagedResult<MemoryFact>(items, totalCount, page, pageSize);
    }
}
