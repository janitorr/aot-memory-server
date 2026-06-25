using AotMemoryServer.Models;
using AotMemoryServer.Application.Abstractions;
using AotMemoryServer.Application.Commands;
using AotMemoryServer.Application.Queries;


namespace AotMemoryServer.Endpoints;

public static class MemoryEndpoints
{
    public static void MapMemoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/memory");

        group.MapGet("/", async (HttpContext context,
            string? category, string? scope, string? key,
            int page = 1, int pageSize = 20) =>
        {
            var handler = context.RequestServices.GetRequiredService<IQueryHandler<GetFacts, PagedResult<MemoryFact>>>();
            var result = await handler.Handle(new GetFacts(category, scope, key, page, pageSize));
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (HttpContext context, int id) =>
        {
            var handler = context.RequestServices.GetRequiredService<IQueryHandler<GetFactById, MemoryFact?>>();
            var result = await handler.Handle(new GetFactById(id));
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapPost("/", async (HttpContext context,
            MemoryFact fact,
            bool force = false) =>
        {
            try
            {
                var handler = context.RequestServices.GetRequiredService<ICommandHandler<UpsertFact, MemoryFact>>();
                var result = await handler.Handle(new UpsertFact(fact, force));
                return Results.Ok(result);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { errors = ex.Errors });
            }
        });

        group.MapPut("/{id:int}", async (HttpContext context,
            int id,
            MemoryFact fact) =>
        {
            try
            {
                var handler = context.RequestServices.GetRequiredService<ICommandHandler<UpdateFact, MemoryFact?>>();
                var result = await handler.Handle(new UpdateFact(id, fact));
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { errors = ex.Errors });
            }
        });

        group.MapDelete("/{id:int}", async (HttpContext context, int id) =>
        {
            var handler = context.RequestServices.GetRequiredService<ICommandHandler<DeleteFact, bool>>();
            var deleted = await handler.Handle(new DeleteFact(id));
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}
