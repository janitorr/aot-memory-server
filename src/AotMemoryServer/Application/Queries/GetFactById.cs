using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Mediator;
using AotMemoryServer.Data;
using AotMemoryServer.Models;
using AotMemoryServer.Application.Abstractions;

namespace AotMemoryServer.Application.Queries;

public sealed record GetFactById(int Id) : IRequest<MemoryFact?>;

public sealed class GetFactByIdHandler(AppDbContext db) : IRequestHandler<GetFactById, MemoryFact?>
{
    public async ValueTask<MemoryFact?> Handle(GetFactById query, CancellationToken cancellationToken)
    {
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync(cancellationToken);

        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT \"Id\", \"Category\", \"Key\", \"Value\", \"Scope\", \"Confidence\", \"Source\", \"UpdatedAt\", \"IsDeprecated\" FROM \"MemoryFacts\" WHERE \"Id\" = @p0";
            cmd.Parameters.Add(new SqliteParameter("@p0", query.Id));

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new MemoryFact
                {
                    Id = reader.GetInt32(0),
                    Category = reader.GetString(1),
                    Key = reader.GetString(2),
                    Value = reader.GetString(3),
                    Scope = reader.GetString(4),
                    Confidence = reader.GetDouble(5),
                    Source = reader.IsDBNull(6) ? null : reader.GetString(6),
                    UpdatedAt = DateTimeOffset.Parse(reader.GetString(7)),
                    IsDeprecated = reader.GetBoolean(8)
                };
            }

            return null;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }
}
