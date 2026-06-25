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

        existing.Category = command.Fact.Category;
        existing.Key = command.Fact.Key;
        existing.Value = command.Fact.Value;
        existing.Scope = command.Fact.Scope;
        existing.Confidence = command.Fact.Confidence;
        existing.Source = command.Fact.Source;
        existing.IsDeprecated = command.Fact.IsDeprecated;
        existing.UpdatedAt = DateTime.UtcNow.ToString("O");

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
