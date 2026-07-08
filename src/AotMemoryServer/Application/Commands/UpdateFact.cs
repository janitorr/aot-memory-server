using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Mediator;
using AotMemoryServer.Data;
using AotMemoryServer.Models;
using AotMemoryServer.Application.Abstractions;

namespace AotMemoryServer.Application.Commands;

public sealed record UpdateFact(int Id, MemoryFact Fact) : IRequest<MemoryFact?>;

public sealed partial class UpdateFactHandler(AppDbContext db, ILogger<UpdateFactHandler> logger)
    : IRequestHandler<UpdateFact, MemoryFact?>
{
    public async ValueTask<MemoryFact?> Handle(UpdateFact command, CancellationToken cancellationToken)
    {
        var errors = MemoryFactValidator.Validate(command.Fact);
        if (errors.Any(e => !e.IsWarning))
            throw new ValidationException(errors);

        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync(cancellationToken);

        try
        {
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT \"Id\", \"Category\", \"Key\", \"Value\", \"Scope\", \"Confidence\", \"Source\", \"UpdatedAt\", \"IsDeprecated\" FROM \"MemoryFacts\" WHERE \"Id\" = @p0";
                cmd.Parameters.Add(new SqliteParameter("@p0", command.Id));

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                    return null;
            }

            command.Fact.Id = command.Id;
            command.Fact.UpdatedAt = DateTimeOffset.UtcNow;

            await using var updateCmd = conn.CreateCommand();
            updateCmd.CommandText = "UPDATE \"MemoryFacts\" SET \"Category\" = @p0, \"Key\" = @p1, \"Value\" = @p2, \"Scope\" = @p3, \"Confidence\" = @p4, \"Source\" = @p5, \"UpdatedAt\" = @p6, \"IsDeprecated\" = @p7 WHERE \"Id\" = @p8";
            updateCmd.Parameters.Add(new SqliteParameter("@p0", command.Fact.Category));
            updateCmd.Parameters.Add(new SqliteParameter("@p1", command.Fact.Key));
            updateCmd.Parameters.Add(new SqliteParameter("@p2", command.Fact.Value));
            updateCmd.Parameters.Add(new SqliteParameter("@p3", command.Fact.Scope));
            updateCmd.Parameters.Add(new SqliteParameter("@p4", command.Fact.Confidence));
            updateCmd.Parameters.Add(new SqliteParameter("@p5", command.Fact.Source ?? (object)DBNull.Value));
            updateCmd.Parameters.Add(new SqliteParameter("@p6", command.Fact.UpdatedAt.ToString("O")));
            updateCmd.Parameters.Add(new SqliteParameter("@p7", command.Fact.IsDeprecated));
            updateCmd.Parameters.Add(new SqliteParameter("@p8", command.Id));
            await updateCmd.ExecuteNonQueryAsync(cancellationToken);

            Log.Updated(logger, command.Id, command.Fact.Category, command.Fact.Key, command.Fact.Scope);
            return command.Fact;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    private static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Updated fact {Id} ({Category}/{Key}/{Scope})")]
        public static partial void Updated(ILogger logger, int id, string category, string key, string scope);
    }
}
