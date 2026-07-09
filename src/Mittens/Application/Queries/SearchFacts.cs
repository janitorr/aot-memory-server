using Mediator;
using Mittens.Data;
using Mittens.Models;
using Mittens.Application.Abstractions;

namespace Mittens.Application.Queries;

public sealed record SearchFacts(string Q, string? Category, string? Scope, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<MittensFact>>;

public sealed class SearchFactsHandler(AppDbContext db) : IRequestHandler<SearchFacts, PagedResult<MittensFact>>
{
    public async ValueTask<PagedResult<MittensFact>> Handle(SearchFacts query, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, Math.Min(100, query.PageSize));

        return await FactReader.SearchAsync(db, query.Q, query.Category, query.Scope, page, pageSize, cancellationToken);
    }
}
