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
        var existing = await CompiledQueries.GetByIdAsync(db, command.Id);

        if (existing is null)
            return false;

        await FactWriter.DeleteAsync(db, command.Id, cancellationToken);

        Log.Deleted(logger, command.Id, existing.Category, existing.Key, existing.Scope);
        return true;
    }

    private static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Deleted fact {Id} ({Category}/{Key}/{Scope})")]
        public static partial void Deleted(ILogger logger, int id, string category, string key, string scope);
    }
}