using Microsoft.EntityFrameworkCore;
using AotMemoryServer.Data;
using AotMemoryServer.Models;
using AotMemoryServer.Application.Abstractions;

namespace AotMemoryServer.Application.Queries;

public sealed record GetFacts(string? Category, string? Scope, string? Key, int Page = 1, int PageSize = 20);

public sealed class GetFactsHandler(AppDbContext db) : IQueryHandler<GetFacts, PagedResult<MemoryFact>>
{
    public async Task<PagedResult<MemoryFact>> Handle(GetFacts query)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, Math.Min(100, query.PageSize));

        var filtered = db.MemoryFacts.AsNoTracking().AsQueryable();

        if (query.Category is not null)
            filtered = filtered.Where(f => f.Category == query.Category);
        if (query.Scope is not null)
            filtered = filtered.Where(f => f.Scope == query.Scope);
        if (query.Key is not null)
            filtered = filtered.Where(f => f.Key == query.Key);

        var totalCount = await filtered.CountAsync();
        var items = await filtered
            .OrderBy(f => f.Category).ThenBy(f => f.Key)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<MemoryFact>(items, totalCount, page, pageSize);
    }
}
