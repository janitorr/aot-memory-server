using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Mediator;
using AotMemoryServer.Data;
using AotMemoryServer.Application.Abstractions;

namespace AotMemoryServer.Application.Commands;

public sealed record DeleteFact(int Id) : IRequest<bool>;

public sealed partial class DeleteFactHandler(AppDbContext db, ILogger<DeleteFactHandler> logger)
    : IRequestHandler<DeleteFact, bool>
{
    public async ValueTask<bool> Handle(DeleteFact command, CancellationToken cancellationToken)
    {
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync(cancellationToken);

        try
        {
            string? category = null;
            string? key = null;
            string? scope = null;

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT \"Category\", \"Key\", \"Scope\" FROM \"MemoryFacts\" WHERE \"Id\" = @p0";
                cmd.Parameters.Add(new SqliteParameter("@p0", command.Id));

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                    return false;

                category = reader.GetString(0);
                key = reader.GetString(1);
                scope = reader.GetString(2);
            }

            await using var delCmd = conn.CreateCommand();
            delCmd.CommandText = "DELETE FROM \"MemoryFacts\" WHERE \"Id\" = @p0";
            delCmd.Parameters.Add(new SqliteParameter("@p0", command.Id));
            await delCmd.ExecuteNonQueryAsync(cancellationToken);

            Log.Deleted(logger, command.Id, category, key, scope);
            return true;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    private static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Deleted fact {Id} ({Category}/{Key}/{Scope})")]
        public static partial void Deleted(ILogger logger, int id, string category, string key, string scope);
    }
}
