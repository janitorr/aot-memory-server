# Mittens — Agent Instructions

**AOT memory server** — persistent key-value facts for AI agents via REST and MCP.

## Tech Stack

- .NET 10, ASP.NET Core minimal APIs
- EF Core + SQLite (no migrations — inline DDL in `Program.cs`)
- Compiled EF model (`Memory/Data/Compiled/`), source-generated JSON (`AppJsonContext`)
- CQRS via `Mediator.SourceGenerator` (`IQueryHandler` / `ICommandHandler`)
- MCP via `ModelContextProtocol.AspNetCore` (stateless HTTP at `/mcp`)
- xUnit for tests

## Commands

```bash
dotnet build                                    # build
dotnet test                                     # all tests (unit + integration)
dotnet test tests/Mittens.Tests.Unit            # unit only
dotnet test tests/Mittens.Tests.Integration     # integration only
dotnet run --project src/Mittens.Host           # dev server on :5070
dotnet publish -c Release                       # AOT native binary (~39 MB)
```

CI order: `restore → build → test → Release build → Release integration test`.

## AOT Constraints

- `PublishAot` and `StripSymbolsAfterPublish` active in **Release only**
- `InvariantGlobalization=true` in Release — no culture-dependent code paths
- No EF migrations; schema is created via `ExecuteSqlRawAsync` at startup
- `NoWarn`: `IL2026`, `IL3050`, `CA1861` (AOT reflection warnings)
- Use source generators, not runtime reflection. `InternalsVisibleTo` for integration tests.

## AOT Data Access (Critical)

EF Core `FromSqlRaw`/`SqlQueryRaw` **do not work** under NativeAOT — they go through the query compiler which requires runtime IL generation. All reads use `SqliteCommand` directly on `AppDbContext.Database.GetDbConnection()`. Writes use `ExecuteSqlInterpolatedAsync` (AOT-safe). Never use `FromSqlRaw`, `SqlQueryRaw`, `.ToListAsync()`, or `.FirstOrDefaultAsync()` on EF Core queries in this project.

## Architecture

```
src/Mittens.Core/         # Pure domain logic (models, interfaces, handlers)
├── Fact/                 # Fact feature (commands, queries, interfaces)
│   ├── Commands/         # Upsert, Update, Delete
│   └── Queries/          # GetFacts, GetFactById, SearchFacts
└── Shared/               # Shared types (PagedResult, ValidationException)

src/Mittens.Host/         # Web application shell
├── Memory/               # Fact feature infrastructure
│   ├── Data/             # DbContext, reader/writer, compiled model
│   │   └── Compiled/     # Precompiled EF Core model
│   └── Endpoints/        # REST and MCP endpoint definitions
├── Endpoints/            # Plumbing (health checks)
├── Serialization/        # JSON serialization context and DTOs
└── Program.cs            # Entry point, DI, routing
```

## Naming Conventions

- Host project: `Mittens.Host` (assembly), root namespace `Mittens.Host`
- Core project: `Mittens.Core` (assembly + namespace)
- Host feature folder is `Memory/` not `Fact/` — avoids collision with `Mittens.Core.Fact.Fact` type
- Database table: `MittensFacts`, schema: `mittens`
- Binary output: `Mittens.Host`

## OpenSpec Workflow

Schema: `spec-driven` (Propose → Spec → Design → Tasks → Implement → Archive).

To start a change, tell me to load the relevant skill:

- **`load openspec-plan`** — I'll explore the idea, then scaffold the change folder with proposal, specs, design, and tasks
- **`load openspec-execute`** — I'll implement the tasks, run tests, and archive

Or just say "I want to make a change" and I'll guide you from there.

## Releases

Semver tags from `main` only: `git tag v1.2.3 && git push origin v1.2.3 && git push origin main`.
CI ignores `*.md`, `openspec/**`, `setup/**` paths. Docker image pushed to `janitorr/mittens`.

## Memory Server (Self-Reference)

This repo **is** the memory server. It listens on `http://localhost:5070`.
- REST: `GET/POST/PUT/DELETE /api/memory`, `GET /api/health`
- MCP: `POST /mcp` (tools: `mittens_list`, `mittens_get`, `mittens_search`, `mittens_set`, `mittens_update`, `mittens_delete`)
- OpenAPI docs: `/scalar/v1`
- Configured as remote MCP in `opencode.json`

For usage instructions (session protocol, categories, when to store), see `setup/SETUP.md` or the global `~/.config/opencode/memory-server.md`.
