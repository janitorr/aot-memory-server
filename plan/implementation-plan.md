# AOT Memory Server — Implementation Plan

## Step 1: Create project skeleton
- `dotnet new web -o src/AotMemoryServer --use-minimal-apis`
- Edit `.csproj`: `PublishAot=true`, `StripSymbolsAfterPublish=true`, target `net10.0`
- Add NuGet packages: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`

## Step 2: Data model & DbContext
- Write `Models/MemoryFact.cs` — entity with 9 properties, data annotations
- Write `Data/AppDbContext.cs` — `OnModelCreating` for indexes + unique constraint
- Write `Data/Compiled/` — EF Core compiled model (AOT requirement, 4 auto-generated files)
- Create initial migration: `dotnet ef migrations add InitialCreate`
- Write `Data/AppDbContextFactory.cs` — design-time factory for EF CLI tooling

## Step 3: Validation (domain logic)
- Write `Models/MemoryFactValidator.cs` — static validation class with source-generated regex:
  - `Validate(MemoryFact)` — runs all checks, returns `IReadOnlyList<ValidationError>`
  - `ResolveConflict(existing, incoming, force)` — confidence comparison, force flag
  - `MaxValueLength = 10_000`
  - `SecretPattern` regex — `sk-` keys, `api_key`, `secret`, `token`, `password`, `private key`
  - `CodePattern` regex — C#/JS/Python markers (soft warnings)
  - Known categories: `preference`, `fact`, `concept`, `rule`, `plan`, `goal`, `task`, `note`
- Write `Models/ValidationError.cs` — `sealed record ValidationError(string Property, string Message, bool IsWarning)`

## Step 4: CQRS command/query handlers
- Write `Application/Abstractions/` — interfaces and shared types:
  - `ICommandHandler<TCommand, TResult>` — `Handle(TCommand)` method
  - `IQueryHandler<TQuery, TResult>` — `Handle(TQuery)` method
  - `PagedResult<T>` — record with `Items`, `TotalCount`, `Page`, `PageSize`
  - `ValidationException` — carries list of `ValidationError`
- Write `Application/Queries/` — query handlers:
  - `GetFacts` / `GetFactsHandler` — filter by category/scope/key, paginated, ordered
  - `GetFactById` / `GetFactByIdHandler` — single lookup by Id
  - `SearchFacts` / `SearchFactsHandler` — LIKE on Key and Value
- Write `Application/Commands/` — command handlers:
  - `UpsertFact` / `UpsertFactHandler` — validates, resolves conflicts, upserts
  - `UpdateFact` / `UpdateFactHandler` — validates, updates existing by Id
  - `DeleteFact` / `DeleteFactHandler` — removes by Id
- All handlers use source-generated `[LoggerMessage]` for structured logging

## Step 5: REST endpoints
- Write `Endpoints/MemoryEndpoints.cs` — extension method `MapMemoryEndpoints`:
  - `GET /memory` — calls `GetFacts` query handler
  - `GET /memory/{id}` — calls `GetFactById` query handler
  - `POST /memory` — calls `UpsertFact` command handler
  - `PUT /memory/{id}` — calls `UpdateFact` command handler
  - `DELETE /memory/{id}` — calls `DeleteFact` command handler
- Write `Endpoints/HealthEndpoints.cs`:
  - `GET /health` — `db.Database.CanConnectAsync()`
  - `GET /ready` — also checks migration applied

## Step 6: MCP endpoint
- Write `Endpoints/McpEndpoints.cs`
- Parse JSON-RPC 2.0 from request body
- Route to CQRS handlers via `method` field
- Return standard JSON-RPC response format

## Step 7: Wire everything in Program.cs
- [done] Register CQRS handlers (DI)
- [done] Register DbContext with SQLite
- [todo] Configure Kestrel binding from env/args
- [todo] Apply migrations on startup
- [todo] Map endpoint groups (REST, Health, MCP)
- [todo] JSON source generator for AOT-safe serialization

## Step 8: Tests
- `tests/AotMemoryServer.Tests.Unit/`:
  - `MemoryFactValidatorTests.cs` — validate length, secrets, code, categories
  - `ConflictResolutionTests.cs` — confidence comparison, force flag
- `tests/AotMemoryServer.Tests.Integration/`:
  - `CustomWebApplicationFactory.cs`
  - `RestEndpointTests.cs` — CRUD via REST
  - `McpEndpointTests.cs` — JSON-RPC dispatch

## Step 9: Dockerfile
- Multi-stage AOT build

## Step 10: docker-compose.yml
- Service definition with volume, port, env vars

## Step 11: Housekeeping
- `.gitignore`, `.env.example`, `README.md`
