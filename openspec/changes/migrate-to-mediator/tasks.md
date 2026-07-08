## 1. Dependencies

- [x] 1.1 Add `Mediator.Abstractions` and `Mediator.SourceGenerator` NuGet packages to `src/AotMemoryServer/AotMemoryServer.csproj` (entry project must reference the source generator)

## 2. Convert Messages & Handlers

- [x] 2.1 Make each request/command record implement `IRequest<TResult>`: `GetFacts`, `GetFactById`, `SearchFacts` (→ `PagedResult<MemoryFact>`), `UpsertFact`, `UpdateFact` (→ `MemoryFact?`/`MemoryFact`), `DeleteFact` (→ `bool`)
- [x] 2.2 Change each handler to implement `IRequestHandler<TRequest, TResult>` (replace `IQueryHandler`/`ICommandHandler`)
- [x] 2.3 Update each handler's `Handle` signature to `public async ValueTask<TResult> Handle(TRequest request, CancellationToken cancellationToken = default)`
- [x] 2.4 Delete `src/AotMemoryServer/Application/Abstractions/ICommandHandler.cs` and `IQueryHandler.cs`; keep `PagedResult.cs` and `ValidationException.cs`

## 3. Wire Up DI

- [x] 3.1 In `Program.cs`, remove the six `builder.Services.AddScoped<IHandler, Handler>()` registrations
- [x] 3.2 Add `builder.Services.AddMediator()` (source-generated DI registration) in their place

## 4. Update Call Sites

- [x] 4.1 In `MemoryMcpTools.cs`, replace the six handler interface fields/constructor params with a single `ISender` and call `await sender.Send(new GetFacts(...))` etc. in each tool
- [x] 4.2 In `MemoryEndpoints.cs`, resolve `ISender` from the request scope and replace `GetRequiredService<IHandler>()` with `await sender.Send(...)`

## 5. Verify AOT & Tests

- [x] 5.1 Run `dotnet build` and confirm no errors
- [x] 5.2 Run `dotnet publish -c Release` (PublishAot=true) and confirm no trimming/reflection warnings
- [x] 5.3 Run unit tests (`tests/AotMemoryServer.Tests.Unit`) and integration tests (`tests/AotMemoryServer.Tests.Integration`); update any references to removed handler interfaces
- [x] 5.4 Run `openspec validate --changes` and `openspec validate --specs`
