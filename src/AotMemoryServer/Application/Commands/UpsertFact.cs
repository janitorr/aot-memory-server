using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Mediator;
using AotMemoryServer.Data;
using AotMemoryServer.Models;
using AotMemoryServer.Application.Abstractions;

namespace AotMemoryServer.Application.Commands;

public sealed record UpsertFact(MemoryFact Fact, bool Force = false) : IRequest<MemoryFact>;

public sealed partial class UpsertFactHandler(AppDbContext db, ILogger<UpsertFactHandler> logger)
    : IRequestHandler<UpsertFact, MemoryFact>
{
    public async ValueTask<MemoryFact> Handle(UpsertFact command, CancellationToken cancellationToken)
    {
        var errors = MemoryFactValidator.Validate(command.Fact);
        if (errors.Any(e => !e.IsWarning))
            throw new ValidationException(errors);

        MemoryFact? existing = null;
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync(cancellationToken);

        try
        {
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT \"Id\", \"Category\", \"Key\", \"Value\", \"Scope\", \"Confidence\", \"Source\", \"UpdatedAt\", \"IsDeprecated\" FROM \"MemoryFacts\" WHERE \"Category\" = @p0 AND \"Key\" = @p1 AND \"Scope\" = @p2";
                cmd.Parameters.Add(new SqliteParameter("@p0", command.Fact.Category));
                cmd.Parameters.Add(new SqliteParameter("@p1", command.Fact.Key));
                cmd.Parameters.Add(new SqliteParameter("@p2", command.Fact.Scope));

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    existing = new MemoryFact
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
            }

            if (existing is not null)
            {
                var resolved = MemoryFactValidator.ResolveConflict(existing, command.Fact, command.Force);
                if (resolved != existing)
                {
                    resolved.UpdatedAt = DateTimeOffset.UtcNow;
                    await using var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE \"MemoryFacts\" SET \"Category\" = @p0, \"Key\" = @p1, \"Value\" = @p2, \"Scope\" = @p3, \"Confidence\" = @p4, \"Source\" = @p5, \"UpdatedAt\" = @p6, \"IsDeprecated\" = @p7 WHERE \"Id\" = @p8";
                    cmd.Parameters.Add(new SqliteParameter("@p0", resolved.Category));
                    cmd.Parameters.Add(new SqliteParameter("@p1", resolved.Key));
                    cmd.Parameters.Add(new SqliteParameter("@p2", resolved.Value));
                    cmd.Parameters.Add(new SqliteParameter("@p3", resolved.Scope));
                    cmd.Parameters.Add(new SqliteParameter("@p4", resolved.Confidence));
                    cmd.Parameters.Add(new SqliteParameter("@p5", resolved.Source ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@p6", resolved.UpdatedAt.ToString("O")));
                    cmd.Parameters.Add(new SqliteParameter("@p7", resolved.IsDeprecated));
                    cmd.Parameters.Add(new SqliteParameter("@p8", existing.Id));
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                    Log.Upserted(logger, resolved.Category, resolved.Key, resolved.Scope);
                    return resolved;
                }

                Log.Upserted(logger, existing.Category, existing.Key, existing.Scope);
                return existing;
            }
            else
            {
                command.Fact.UpdatedAt = DateTimeOffset.UtcNow;
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO \"MemoryFacts\" (\"Category\", \"Key\", \"Value\", \"Scope\", \"Confidence\", \"Source\", \"UpdatedAt\", \"IsDeprecated\") VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7); SELECT last_insert_rowid();";
                cmd.Parameters.Add(new SqliteParameter("@p0", command.Fact.Category));
                cmd.Parameters.Add(new SqliteParameter("@p1", command.Fact.Key));
                cmd.Parameters.Add(new SqliteParameter("@p2", command.Fact.Value));
                cmd.Parameters.Add(new SqliteParameter("@p3", command.Fact.Scope));
                cmd.Parameters.Add(new SqliteParameter("@p4", command.Fact.Confidence));
                cmd.Parameters.Add(new SqliteParameter("@p5", command.Fact.Source ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqliteParameter("@p6", command.Fact.UpdatedAt.ToString("O")));
                cmd.Parameters.Add(new SqliteParameter("@p7", command.Fact.IsDeprecated));
                var insertedId = await cmd.ExecuteScalarAsync(cancellationToken);
                command.Fact.Id = insertedId is long l ? (int)l : 0;
                Log.Upserted(logger, command.Fact.Category, command.Fact.Key, command.Fact.Scope);
                return command.Fact;
            }
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    private static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Upserted fact {Category}/{Key}/{Scope}")]
        public static partial void Upserted(ILogger logger, string category, string key, string scope);
    }
}
