using Microsoft.EntityFrameworkCore;
using AotMemoryServer.Data;
using AotMemoryServer.Application.Abstractions;

namespace AotMemoryServer.Application.Commands;

public sealed record DeleteFact(int Id);

public sealed partial class DeleteFactHandler(AppDbContext db, ILogger<DeleteFactHandler> logger)
    : ICommandHandler<DeleteFact, bool>
{
    public async Task<bool> Handle(DeleteFact command)
    {
        var existing = await db.MemoryFacts.FindAsync(command.Id);
        if (existing is null)
            return false;

        db.MemoryFacts.Remove(existing);
        await db.SaveChangesAsync();
        Log.Deleted(logger, existing.Id, existing.Category, existing.Key, existing.Scope);
        return true;
    }

    private static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Deleted fact {Id} ({Category}/{Key}/{Scope})")]
        public static partial void Deleted(ILogger logger, int id, string category, string key, string scope);
    }
}
