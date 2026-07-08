## Context

The server currently uses a custom, hand-written CQRS layer:

- `Application/Abstractions/ICommandHandler.cs` — `ICommandHandler<TCommand, TResult>` with `Handle(...)`
- `Application/Abstractions/IQueryHandler.cs` — `IQueryHandler<TQuery, TResult>` with `Handle(...)`
- Six handler classes (`GetFactsHandler`, `GetFactByIdHandler`, `SearchFactsHandler`, `UpsertFactHandler`, `UpdateFactHandler`, `DeleteFactHandler`) each implement one of those interfaces and keep their existing constructor (e.g. `GetFactsHandler(AppDbContext db)`, `UpsertFactHandler(AppDbContext db, ILogger<...> logger)`).
- `Program.cs` manually registers each handler: `builder.Services.AddScoped<IQueryHandler<GetFacts, PagedResult<MemoryFact>>, GetFactsHandler>();` × 6.
- `MemoryMcpTools` constructor takes all 6 handler interfaces and stores them in fields.
- `MemoryEndpoints` resolves handlers per-request via `context.RequestServices.GetRequiredService<IQueryHandler<...>>()` / `ICommandHandler<...>()`.

This works but grows linearly with every new use case and bloats `MemoryMcpTools`'s constructor ("fat constructor" problem). The project is `PublishAot=true` (.NET 10) and already avoids reflection elsewhere, so any mediator must also be AOT-safe.

The `martinothamar/Mediator` library (NuGet `Mediator.Abstractions` + `Mediator.SourceGenerator`) implements the mediator pattern via **source generators**: it generates `IMediator`/`ISender` implementations and the DI registrations at compile time, with full Native AOT support (no reflection, no runtime codegen). It emits build-time diagnostics if a handler is missing.

## Goals / Non-Goals

**Goals:**
- Replace the custom handler interfaces with Mediator's `IRequest<T>` / `IRequestHandler<T, TResult>`.
- Collapse the 6 `AddScoped` registrations into a single `builder.Services.AddMediator()`.
- Reduce `MemoryMcpTools` injection from 6 handler interfaces to a single `IMediator`/`ISender`.
- Update `MemoryEndpoints` to send requests through `IMediator`/`ISender` instead of resolving handler interfaces.
- Preserve AOT compatibility (`PublishAot=true` build must pass with no trimming/reflection warnings).
- Keep the existing request/command records and handler logic (SQL, validation, conflict resolution) intact.

**Non-Goals:**
- No change to REST API contracts, MCP tool contracts, storage schema, or validation behavior.
- No introduction of pipeline behaviors (logging/metrics) — handlers already use `[LoggerMessage]`; out of scope unless desired later.
- No change to the public fact data model.

## Decisions

### D1. Use `martinothamar/Mediator` source-generated mediator
**Rationale:** It is the only mainstream mediator option with first-class Native AOT support via source generation (MediatR relies on reflection/DI open generics that are problematic under AOT). It directly solves the "fat constructor" problem and removes manual DI wiring.
**Alternatives considered:**
- *Keep custom abstractions* — simplest, but doesn't reduce the injection surface; boilerplate grows with each new use case.
- *MediatR* — popular but historically reflection-based; not ideal for `PublishAot=true`.

### D2. `IRequest<TResponse>` message records + `IRequestHandler<T, TResponse>`
Each existing record (`GetFacts`, `GetFactById`, `SearchFacts`, `UpsertFact`, `UpdateFact`, `DeleteFact`) implements `IRequest<TResult>` where `TResult` matches the current handler return type (`PagedResult<MemoryFact>`, `MemoryFact?`, `MemoryFact`, `bool`). Handlers change from `: IQueryHandler<GetFacts, PagedResult<MemoryFact>>` to `: IRequestHandler<GetFacts, PagedResult<MemoryFact>>` and rename `Handle(...)` to the Mediator signature `public async Task<TResult> Handle(TRequest request, CancellationToken cancellationToken = default)`.
**Rationale:** Minimal churn — records and handler bodies stay the same; only the implemented interface and `Handle` signature change.

### D3. `AddMediator()` replaces manual registrations
Remove the six `AddScoped<...>()` lines; add `builder.Services.AddMediator()`. The source generator discovers handlers in referenced assemblies and emits DI registrations (scoped by default) — required for AOT (no open-generic runtime registration).
**Rationale:** Single line, AOT-safe, build-time verified.

### D4. Inject `IMediator` (or `ISender`) into call sites
- `MemoryMcpTools`: constructor takes `IMediator mediator` (or `ISender`); each tool calls `await mediator.Send(new GetFacts(...))`. `IMediator` exposes `Send`; `ISender` is the minimal interface if only sending is needed. Use `IMediator` for simplicity.
- `MemoryEndpoints`: resolve `IMediator` from `context.RequestServices` (or add it as a route-handler parameter) and `await mediator.Send(...)`.
**Rationale:** One dependency instead of six; new tools/endpoints just send new request types without touching DI or constructors.

### D5. Caching mode & lifetime for AOT
Use default `CachingMode.Eager` (fine for a long-running server) and accept the generated singleton `IMediator` (handlers remain scoped; the mediator resolves them per request via the scope). Verify the AOT publish build emits no trimming/reflection warnings. If cold-start matters, switch to `CachingMode.Lazy` via the `MediatorOptions` assembly attribute — note as a follow-up, not required now.
**Rationale:** Defaults are correct for this always-on server; keeps the change small.

## Risks / Trade-offs

- [Risk] Source generator doesn't discover handlers because the entry project doesn't reference the assembly containing them → Mitigation: ensure `Mediator.SourceGenerator` is referenced by the entry project (`AotMemoryServer`) and handler project is referenced; the generator scans referenced assemblies. Build-time diagnostics will warn if a request lacks a handler.
- [Risk] AOT publish fails or emits trim/reflection warnings → Mitigation: run `dotnet publish` with `PublishAot=true`; confirm clean build; the library is designed for AOT so this is unlikely, but verify.
- [Risk] `CancellationToken` signature change on `Handle` breaks callers/tests → Mitigation: add optional `CancellationToken = default` parameter; existing direct `new Handler(db).Handle(req)` calls in unit tests still compile (token optional).
- [Risk] Integration tests reference `IQueryHandler`/`ICommandHandler` types → Mitigation: update those references to `IMediator.Send` or construct handlers directly (handlers keep public constructors).
- [Trade-off] Adds two NuGet dependencies and a compile-time codegen step; slightly more build complexity in exchange for far less runtime wiring and a cleaner constructor.

## Migration Plan

1. Add `Mediator.Abstractions` + `Mediator.SourceGenerator` to `AotMemoryServer.csproj`.
2. Change each request record to implement `IRequest<TResult>`; change each handler to implement `IRequestHandler<TRequest, TResult>` with the `Handle(TRequest, CancellationToken)` signature.
3. Delete `Application/Abstractions/ICommandHandler.cs` and `IQueryHandler.cs`. Keep `PagedResult` and `ValidationException` in `Application/Abstractions` (still used by handlers/DTOs).
4. In `Program.cs`, replace the six `AddScoped` lines with `builder.Services.AddMediator()`.
5. Update `MemoryMcpTools` to inject `IMediator` and call `mediator.Send(...)`.
6. Update `MemoryEndpoints` to use `IMediator.Send(...)`.
7. `dotnet build` then `dotnet publish -c Release` (AOT) — confirm no warnings/errors.
8. Run unit + integration tests.

**Rollback:** revert the change commit; the pre-change code still compiles independently. No data/schema change, so rollback is safe.

## Open Questions

- None blocking. Optional follow-up: add a `IPipelineBehavior` for structured request logging via the mediator pipeline (handlers already log via `[LoggerMessage]`).
