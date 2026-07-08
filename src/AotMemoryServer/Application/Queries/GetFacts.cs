using Mediator;
using AotMemoryServer.Data;
using AotMemoryServer.Models;
using AotMemoryServer.Application.Abstractions;

namespace AotMemoryServer.Application.Queries;

public sealed record GetFacts(string? Category, string? Scope, string? Key, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<MemoryFact>>;

public sealed class GetFactsHandler(AppDbContext db) : IRequestHandler<GetFacts, PagedResult<MemoryFact>>
{
    public async ValueTask<PagedResult<MemoryFact>> Handle(GetFacts query, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, Math.Min(100, query.PageSize));

        var totalCount = await CompiledQueries.GetFactsCountAsync(db, query.Category, query.Scope, query.Key);
        var offset = (page - 1) * pageSize;
        var items = await CompiledQueries.GetFactsPageAsync(db, query.Category, query.Scope, query.Key, pageSize, offset);

        return new PagedResult<MemoryFact>(items, totalCount, page, pageSize);
    }
}