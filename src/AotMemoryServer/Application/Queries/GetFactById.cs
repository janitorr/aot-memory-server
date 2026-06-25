using Microsoft.EntityFrameworkCore;
using AotMemoryServer.Data;
using AotMemoryServer.Models;
using AotMemoryServer.Application.Abstractions;

namespace AotMemoryServer.Application.Queries;

public sealed record GetFactById(int Id);

public sealed class GetFactByIdHandler(AppDbContext db) : IQueryHandler<GetFactById, MemoryFact?>
{
    public async Task<MemoryFact?> Handle(GetFactById query)
    {
        return await db.MemoryFacts.AsNoTracking().FirstOrDefaultAsync(f => f.Id == query.Id);
    }
}
