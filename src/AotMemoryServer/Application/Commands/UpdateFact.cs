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

        var existing = await CompiledQueries.GetByIdAsync(db, command.Id);

        if (existing is null)
            return null;

        command.Fact.Id = command.Id;
        command.Fact.UpdatedAt = DateTimeOffset.UtcNow;

        await FactWriter.UpdateAsync(db, command.Fact, command.Id, cancellationToken);

        Log.Updated(logger, command.Id, command.Fact.Category, command.Fact.Key, command.Fact.Scope);
        return command.Fact;
    }

    private static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Updated fact {Id} ({Category}/{Key}/{Scope})")]
        public static partial void Updated(ILogger logger, int id, string category, string key, string scope);
    }
}