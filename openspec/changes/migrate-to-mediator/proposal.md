## Why

The application uses a hand-rolled CQRS abstraction (`ICommandHandler`/`IQueryHandler`) with manual per-handler DI registrations in `Program.cs` (6 calls) and a 6-parameter constructor in `MemoryMcpTools`. This is boilerplate that scales poorly and clutters the constructor. The `martinothamar/Mediator` source-generated library provides the same mediator pattern with **full Native AOT support** (no reflection, no runtime codegen) and collapses all handler wiring into a single `AddMediator()` call plus a single `IMediator`/`ISender` dependency at the call sites. Migrating now removes the custom abstractions and reduces injection surface ahead of future feature growth.

## What Changes

- Add NuGet packages `Mediator.Abstractions` and `Mediator.SourceGenerator`.
- Replace custom `ICommandHandler<TCommand, TResult>` / `IQueryHandler<TQuery, TResult>` interfaces and handler base types with Mediator's `IRequest<TResponse>` message records and `IRequestHandler<TRequest, TResponse>` implementations.
- Remove the 6 manual `builder.Services.AddScoped<IHandler, Handler>()` registrations in `Program.cs`; replace with a single `builder.Services.AddMediator()` (source-generated DI).
- Update `MemoryMcpTools` to inject a single `IMediator` (or `ISender`) instead of 6 handler interfaces; route each tool through `mediator.Send(...)`.
- Update `MemoryEndpoints` to resolve `IMediator`/`ISender` from the request scope and call `Send(...)` instead of `GetRequiredService<IQueryHandler<...>>()` / `ICommandHandler<...>()`.
- Keep request/command records (`GetFacts`, `UpsertFact`, etc.) and handler implementations; change only the interface they implement and the `Handle` signature to match `IRequestHandler`.
- **BREAKING**: public surface of `Application.Abstractions` (`ICommandHandler`, `IQueryHandler`, `PagedResult`, `ValidationException`) is removed; any external caller referencing those interfaces must update. No REST/MCP contract change — request/response shapes are unchanged.

## Capabilities

### New Capabilities
<!-- none — this change is internal refactoring, no new externally-visible capability -->

### Modified Capabilities
<!-- No requirement-level behavior changes; the REST API and MCP tool contracts (see specs rest-api, mcp-server, memory-fact-store) are unchanged. Only implementation wiring changes. Leave empty. -->

## Impact

- **Dependencies**: new `Mediator.Abstractions` and `Mediator.SourceGenerator` NuGet packages; project must reference the source generator for AOT codegen.
- **Code**: `Application/Abstractions/ICommandHandler.cs` and `IQueryHandler.cs` deleted; `PagedResult` and `ValidationException` relocated (they are still used by handlers and DTOs — keep in `Application/Abstractions` but decoupled from handler interfaces). All 6 handler classes change their base interface. `Program.cs`, `MemoryMcpTools.cs`, `MemoryEndpoints.cs` updated.
- **AOT**: Mediator uses source generation + `CachingMode` (Eager default, or Lazy for cold-start AOT) and singleton `IMediator` recommended — must verify `PublishAot=true` build still succeeds and no reflection warnings appear.
- **Tests**: unit tests that constructed handlers directly (e.g. `new GetFactsHandler(db)`) still work since handler classes keep their constructors; integration tests referencing handler interfaces may need updates.
- **Risk**: source generator scans assemblies for handlers — ensure all handler assemblies are referenced by the entry project so generation picks them up.
