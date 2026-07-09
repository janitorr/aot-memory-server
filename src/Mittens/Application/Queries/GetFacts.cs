using Mediator;
using Mittens.Data;
using Mittens.Models;
using Mittens.Application.Abstractions;

namespace Mittens.Application.Queries;

public sealed record GetFacts(string? Category, string? Scope, string? Key, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<MittensFact>>;

public sealed class GetFactsHandler(AppDbContext db) : IRequestHandler<GetFacts, PagedResult<MittensFact>>
{
    public async ValueTask<PagedResult<MittensFact>> Handle(GetFacts query, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, Math.Min(100, query.PageSize));

        return await FactReader.ListAsync(db, query.Category, query.Scope, query.Key, page, pageSize, cancellationToken);
    }
}
