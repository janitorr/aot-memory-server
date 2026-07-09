using Microsoft.EntityFrameworkCore;
using Mittens.Models;
using Mittens.Application.Abstractions;

namespace Mittens.Data;

public static class FactReader
{
    public static async Task<MittensFact?> GetByIdAsync(AppDbContext db, int id, CancellationToken ct)
    {
        return await db.MittensFacts.FromSqlRaw(MittensFactSql.GetById, id)
            .FirstOrDefaultAsync(ct);
    }

    public static async Task<MittensFact?> GetByCategoryKeyScopeAsync(AppDbContext db, string category, string key, string scope, CancellationToken ct)
    {
        return await db.MittensFacts.FromSqlRaw(MittensFactSql.GetByCategoryKeyScope, category, key, scope)
            .FirstOrDefaultAsync(ct);
    }

    public static async Task<PagedResult<MittensFact>> ListAsync(AppDbContext db, string? category, string? scope, string? key, int page, int pageSize, CancellationToken ct)
    {
        var offset = (page - 1) * pageSize;

        var totalCount = await db.Database.SqlQueryRaw<int>(MittensFactSql.GetFactsCount,
            category ?? (object)DBNull.Value,
            scope ?? (object)DBNull.Value,
            key ?? (object)DBNull.Value)
            .FirstOrDefaultAsync(ct);

        var items = await db.MittensFacts.FromSqlRaw(MittensFactSql.GetFactsPage,
            category ?? (object)DBNull.Value,
            scope ?? (object)DBNull.Value,
            key ?? (object)DBNull.Value,
            pageSize,
            offset)
            .ToListAsync(ct);

        return new PagedResult<MittensFact>(items, totalCount, page, pageSize);
    }

    public static async Task<PagedResult<MittensFact>> SearchAsync(AppDbContext db, string q, string? category, string? scope, int page, int pageSize, CancellationToken ct)
    {
        var offset = (page - 1) * pageSize;

        var totalCount = await db.Database.SqlQueryRaw<int>(MittensFactSql.SearchFactsCount,
            q,
            category ?? (object)DBNull.Value,
            scope ?? (object)DBNull.Value)
            .FirstOrDefaultAsync(ct);

        var items = await db.MittensFacts.FromSqlRaw(MittensFactSql.SearchFactsPage,
            q,
            category ?? (object)DBNull.Value,
            scope ?? (object)DBNull.Value,
            pageSize,
            offset)
            .ToListAsync(ct);

        return new PagedResult<MittensFact>(items, totalCount, page, pageSize);
    }
}
