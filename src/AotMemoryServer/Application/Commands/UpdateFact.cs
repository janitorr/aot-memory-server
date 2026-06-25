using Microsoft.EntityFrameworkCore;
using AotMemoryServer.Data;
using AotMemoryServer.Models;
using AotMemoryServer.Application.Abstractions;

namespace AotMemoryServer.Application.Commands;

public sealed record UpdateFact(int Id, MemoryFact Fact);

public sealed partial class UpdateFactHandler(AppDbContext db, ILogger<UpdateFactHandler> logger)
    : ICommandHandler<UpdateFact, MemoryFact?>
{
    public async Task<MemoryFact?> Handle(UpdateFact command)
    {
        var errors = MemoryFactValidator.Validate(command.Fact);
        if (errors.Any(e => !e.IsWarning))
            throw new ValidationException(errors);

        var existing = await db.MemoryFacts.FindAsync(command.Id);
        if (existing is null)
            return null;

        command.Fact.Id = existing.Id;
        command.Fact.UpdatedAt = DateTimeOffset.UtcNow;
        db.Entry(existing).CurrentValues.SetValues(command.Fact);

        await db.SaveChangesAsync();
        Log.Updated(logger, existing.Id, existing.Category, existing.Key, existing.Scope);
        return existing;
    }

    private static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Updated fact {Id} ({Category}/{Key}/{Scope})")]
        public static partial void Updated(ILogger logger, int id, string category, string key, string scope);
    }
}
