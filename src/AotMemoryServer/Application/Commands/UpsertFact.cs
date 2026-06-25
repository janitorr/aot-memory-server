using Microsoft.EntityFrameworkCore;
using AotMemoryServer.Data;
using AotMemoryServer.Models;
using AotMemoryServer.Application.Abstractions;

namespace AotMemoryServer.Application.Commands;

public sealed record UpsertFact(MemoryFact Fact, bool Force = false);

public sealed partial class UpsertFactHandler(AppDbContext db, ILogger<UpsertFactHandler> logger)
    : ICommandHandler<UpsertFact, MemoryFact>
{
    public async Task<MemoryFact> Handle(UpsertFact command)
    {
        var errors = MemoryFactValidator.Validate(command.Fact);
        if (errors.Any(e => !e.IsWarning))
            throw new ValidationException(errors);

        var existing = await db.MemoryFacts
            .FirstOrDefaultAsync(f =>
                f.Category == command.Fact.Category &&
                f.Key == command.Fact.Key &&
                f.Scope == command.Fact.Scope);

        MemoryFact result;
        if (existing is not null)
        {
            result = MemoryFactValidator.ResolveConflict(existing, command.Fact, command.Force);
            if (result != existing)
            {
                result.UpdatedAt = DateTimeOffset.UtcNow;
                db.Entry(existing).CurrentValues.SetValues(result);
            }
        }
        else
        {
            command.Fact.UpdatedAt = DateTimeOffset.UtcNow;
            db.MemoryFacts.Add(command.Fact);
            result = command.Fact;
        }

        await db.SaveChangesAsync();
        Log.Upserted(logger, result.Category, result.Key, result.Scope);
        return result;
    }

    private static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Upserted fact {Category}/{Key}/{Scope}")]
        public static partial void Upserted(ILogger logger, string category, string key, string scope);
    }
}
