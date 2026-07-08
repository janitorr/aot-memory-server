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

        var existing = await CompiledQueries.GetByCategoryKeyScopeAsync(db, command.Fact.Category, command.Fact.Key, command.Fact.Scope);

        if (existing is not null)
        {
            var resolved = MemoryFactValidator.ResolveConflict(existing, command.Fact, command.Force);
            if (resolved != existing)
            {
                resolved.UpdatedAt = DateTimeOffset.UtcNow;
                await FactWriter.UpdateAsync(db, resolved, existing.Id, cancellationToken);
                Log.Upserted(logger, resolved.Category, resolved.Key, resolved.Scope);
                return resolved;
            }

            Log.Upserted(logger, existing.Category, existing.Key, existing.Scope);
            return existing;
        }
        else
        {
            await FactWriter.InsertAsync(db, command.Fact, cancellationToken);
            Log.Upserted(logger, command.Fact.Category, command.Fact.Key, command.Fact.Scope);
            return command.Fact;
        }
    }

    private static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Upserted fact {Category}/{Key}/{Scope}")]
        public static partial void Upserted(ILogger logger, string category, string key, string scope);
    }
}