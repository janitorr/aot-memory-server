using Mediator;
using AotMemoryServer.Data;
using AotMemoryServer.Models;
using AotMemoryServer.Application.Abstractions;

namespace AotMemoryServer.Application.Queries;

public sealed record SearchFacts(string Q, string? Category, string? Scope, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<MemoryFact>>;

public sealed class SearchFactsHandler(AppDbContext db) : IRequestHandler<SearchFacts, PagedResult<MemoryFact>>
{
    public async ValueTask<PagedResult<MemoryFact>> Handle(SearchFacts query, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, Math.Min(100, query.PageSize));
        var totalCount = await CompiledQueries.SearchFactsCountAsync(db, query.Q, query.Category, query.Scope);
        var offset = (page - 1) * pageSize;
        var items = await CompiledQueries.SearchFactsPageAsync(db, query.Q, query.Category, query.Scope, pageSize, offset);

        return new PagedResult<MemoryFact>(items, totalCount, page, pageSize);
    }
}