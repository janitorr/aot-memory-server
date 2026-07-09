using Mittens.Application.Abstractions;
using Mittens.Application.Commands;
using Mittens.Application.Queries;
using Mittens.Application.Serialization;
using Mittens.Models;
using Mediator;

namespace Mittens.Endpoints;

public static class MittensEndpoints
{
    public static void MapMittensEndpoints(this WebApplication app)
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
            MittensFact fact,
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
            MittensFact fact) =>
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
