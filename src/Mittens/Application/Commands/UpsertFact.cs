using Mediator;
using Mittens.Data;
using Mittens.Models;
using Mittens.Application.Abstractions;

namespace Mittens.Application.Commands;

public sealed record UpsertFact(MittensFact Fact, bool Force = false) : IRequest<MittensFact>;

public sealed partial class UpsertFactHandler(AppDbContext db, ILogger<UpsertFactHandler> logger)
    : IRequestHandler<UpsertFact, MittensFact>
{
    public async ValueTask<MittensFact> Handle(UpsertFact command, CancellationToken cancellationToken)
    {
        var errors = MittensFactValidator.Validate(command.Fact);
        if (errors.Any(e => !e.IsWarning))
            throw new ValidationException(errors);

        var existing = await FactReader.GetByCategoryKeyScopeAsync(db, command.Fact.Category, command.Fact.Key, command.Fact.Scope, cancellationToken);

        if (existing is not null)
        {
            var resolved = MittensFactValidator.ResolveConflict(existing, command.Fact, command.Force);
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
