using Mediator;
using AotMemoryServer.Data;
using AotMemoryServer.Models;

namespace AotMemoryServer.Application.Queries;

public sealed record GetFactById(int Id) : IRequest<MemoryFact?>;

public sealed class GetFactByIdHandler(AppDbContext db) : IRequestHandler<GetFactById, MemoryFact?>
{
    public async ValueTask<MemoryFact?> Handle(GetFactById query, CancellationToken cancellationToken)
    {
        return await CompiledQueries.GetByIdAsync(db, query.Id);
    }
}