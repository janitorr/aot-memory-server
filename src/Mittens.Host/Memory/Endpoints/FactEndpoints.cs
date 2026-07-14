using Mittens.Core.Fact;
using Mittens.Core.Fact.Commands;
using Mittens.Core.Fact.Queries;
using Mittens.Core.Shared;
using Mittens.Host.Serialization;
using Mediator;

namespace Mittens.Host.Memory.Endpoints;

public static class FactEndpoints
{
    public static void MapFactEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/memory");

        group.MapGet("/", async (HttpContext context,
            string? category, string? scope, string? key,
            int page = 1, int pageSize = 20) =>
        {
            var sender = context.RequestServices.GetRequiredService<ISender>();
            var result = await sender.Send(new GetFacts(category, scope, key, page, pageSize));
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (HttpContext context, int id) =>
        {
            var sender = context.RequestServices.GetRequiredService<ISender>();
            var result = await sender.Send(new GetFactById(id));
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapPost("/", async (HttpContext context,
            Fact fact,
            bool force = false) =>
        {
            try
            {
                var sender = context.RequestServices.GetRequiredService<ISender>();
                var result = await sender.Send(new UpsertFact(fact, force));
                return Results.Ok(result);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new ErrorResponse(ex.Errors));
            }
        });

        group.MapPut("/{id:int}", async (HttpContext context,
            int id,
            Fact fact) =>
        {
            try
            {
                var sender = context.RequestServices.GetRequiredService<ISender>();
                var result = await sender.Send(new UpdateFact(id, fact));
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new ErrorResponse(ex.Errors));
            }
        });

        group.MapDelete("/{id:int}", async (HttpContext context, int id) =>
        {
            var sender = context.RequestServices.GetRequiredService<ISender>();
            var deleted = await sender.Send(new DeleteFact(id));
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}
