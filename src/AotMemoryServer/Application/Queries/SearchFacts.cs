using Microsoft.EntityFrameworkCore;
using AotMemoryServer.Data;
using AotMemoryServer.Models;
using AotMemoryServer.Application.Abstractions;

namespace AotMemoryServer.Application.Queries;

public sealed record SearchFacts(string Q, string? Category, string? Scope, int Page = 1, int PageSize = 20);

public sealed class SearchFactsHandler(AppDbContext db) : IQueryHandler<SearchFacts, PagedResult<MemoryFact>>
{
    public async Task<PagedResult<MemoryFact>> Handle(SearchFacts query)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, Math.Min(100, query.PageSize));

        var filtered = db.MemoryFacts.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var pattern = $"%{query.Q}%";
            filtered = filtered.Where(f =>
                EF.Functions.Like(f.Key, pattern) ||
                EF.Functions.Like(f.Value, pattern));
        }

        if (query.Category is not null)
            filtered = filtered.Where(f => f.Category == query.Category);
        if (query.Scope is not null)
            filtered = filtered.Where(f => f.Scope == query.Scope);

        var totalCount = await filtered.CountAsync();
        var items = await filtered
            .OrderBy(f => f.Category).ThenBy(f => f.Key)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<MemoryFact>(items, totalCount, page, pageSize);
    }
}
