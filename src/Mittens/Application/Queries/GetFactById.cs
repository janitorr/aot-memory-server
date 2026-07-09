using Mediator;
using Mittens.Data;
using Mittens.Models;

namespace Mittens.Application.Queries;

public sealed record GetFactById(int Id) : IRequest<MittensFact?>;

public sealed class GetFactByIdHandler(AppDbContext db) : IRequestHandler<GetFactById, MittensFact?>
{
    public async ValueTask<MittensFact?> Handle(GetFactById query, CancellationToken cancellationToken)
    {
        return await FactReader.GetByIdAsync(db, query.Id, cancellationToken);
    }
}
