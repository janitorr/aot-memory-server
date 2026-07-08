# AOT Memory Server

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com)
[![.NET](https://github.com/janitorr/aot-memory-server/actions/workflows/dotnet.yml/badge.svg)](https://github.com/janitorr/aot-memory-server/actions/workflows/dotnet.yml)

A lightweight, AOT-compiled persistent memory store for AI agents. Provides REST and MCP APIs backed by SQLite. Compiled to a native binary (~33 MB) with fast startup and zero runtime dependencies.

## Features

- **Native AOT binary** — no .NET runtime required, instant startup
- **REST API** — full CRUD at `/api/memory`
- **MCP endpoint** — AI-agent-friendly endpoint at `/mcp`
- **Health checks** — `/api/health` and `/api/ready`
- **OpenAPI + Scalar UI** — interactive docs at `/scalar/v1`
- **Validation** — input validation, secret detection, conflict resolution
- **Search** — full-text search across keys and values
- **Pagination & filtering** — by category, scope, and key
- **CQRS architecture** — clean separation of commands and queries
- **34+ tests** — unit + integration

## Quick Start

```bash
# Run the server
dotnet run --project src/AotMemoryServer

# Run tests
dotnet test

# AOT-publish a native binary
dotnet publish -c Release
```

The server starts at `http://localhost:5070`. Open `http://localhost:5070/scalar/v1` for the interactive API reference.

### Docker

```bash
# Build and start
docker compose up -d

# Follow logs
docker compose logs -f

# Stop
docker compose down

# Stop and remove the data volume
docker compose down -v
```

The container stores the SQLite database in a named volume (`memory-data`), so data persists across restarts. Works on Linux, macOS, and Windows (Docker Desktop).

For an LLM-assisted setup in your own project, see [`SETUP.md`](SETUP.md).

## API Summary

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/memory` | List facts (filters: `category`, `scope`, `key`, `page`, `pageSize`) |
| GET | `/api/memory/{id}` | Get a fact by ID |
| POST | `/api/memory` | Create or upsert a fact (`?force=true` to overwrite conflicts) |
| PUT | `/api/memory/{id}` | Update a fact by ID |
| DELETE | `/api/memory/{id}` | Delete a fact by ID |
| GET | `/api/health` | Health check |
| GET | `/api/ready` | Readiness check |
| POST | `/mcp` | MCP (tools: `memory_list`, `memory_get`, `memory_search`, `memory_set`, `memory_update`, `memory_delete`) |

Full API documentation with curl examples is in [`AGENTS.md`](AGENTS.md). Interactive docs are at `/scalar/v1` when the server is running.

## Configuration

| Environment variable | Default | Description |
|---------------------|---------|-------------|
| `ASPNETCORE_URLS` | `http://0.0.0.0:5070` | Kestrel binding address |
| `ConnectionStrings__DefaultDb` | `Data Source=memory.db` | SQLite connection string |

Configuration is managed through `appsettings.json` / `appsettings.Development.json`.

## Memory Fact Model

```json
{
  "id": 0,
  "category": "fact",
  "key": "my-key",
  "value": "store any text here",
  "scope": "project",
  "confidence": 0.9,
  "source": null,
  "updatedAt": "2026-06-25T12:00:00Z",
  "isDeprecated": false
}
```

| Field | Type | Description |
|-------|------|-------------|
| `id` | int | Auto-generated primary key |
| `category` | string | One of: `preference`, `fact`, `concept`, `rule`, `plan`, `goal`, `task`, `note` |
| `key` | string | Unique within `(category, scope)` |
| `value` | string | The stored content (max 10,000 chars) |
| `scope` | string | Feature or area name (e.g. `auth`, `api`, `project`) |
| `confidence` | double | 0.0–1.0, used for conflict resolution |
| `source` | string? | Optional identifier |
| `updatedAt` | string | ISO 8601 timestamp |
| `isDeprecated` | bool | Soft-delete flag |

## Project Structure

```
src/AotMemoryServer/
├── Application/          # CQRS handlers, serialization, DTOs
│   ├── Abstractions/     # Interfaces and base types
│   ├── Commands/         # Upsert, Update, Delete
│   ├── Queries/          # GetFacts, GetFactById, SearchFacts
│   └── Serialization/    # JSON serialization context and DTOs
├── Data/                 # EF Core DbContext and compiled model
│   └── Compiled/         # Precompiled EF Core model (AOT-ready)
├── Endpoints/            # REST, Health, MCP endpoint definitions
├── Models/               # MemoryFact entity, validator, validation errors
└── Program.cs            # Entry point, DI, middleware, routing

tests/
├── AotMemoryServer.Tests.Unit/        # Validator & conflict resolution tests
└── AotMemoryServer.Tests.Integration/  # REST, MCP, health endpoint tests
```

## Development

```bash
# Build
dotnet build

# Run unit tests
dotnet test tests/AotMemoryServer.Tests.Unit

# Run integration tests
dotnet test tests/AotMemoryServer.Tests.Integration
```

## Using with opencode

### 1. Run the server

Copy [`docker-compose.example.yml`](docker-compose.example.yml) into your project and start it:

```bash
docker compose -f docker-compose.example.yml up -d
```

Or download it directly:

```bash
curl -O https://raw.githubusercontent.com/janitorr/aot-memory-server/main/docker-compose.example.yml
docker compose -f docker-compose.example.yml up -d
```

The server listens at `http://localhost:5070`.

### 2. Configure opencode

Add this to your project's `opencode.json`:

```json
{
  "$schema": "https://opencode.ai/config.json",
  "mcp": {
    "memory": {
      "type": "remote",
      "url": "http://localhost:5070/mcp",
      "enabled": true
    }
  }
}
```

### 3. Add agent instructions

Copy [`AGENTS.md`](AGENTS.md) into your project root. It tells opencode agents about available tools, categories, scope conventions, and startup — so they know when and how to use the memory server.

### Agent setup instructions

If you are an AI agent helping a user install this memory server for their project, read [`SETUP.md`](SETUP.md) for the complete step-by-step installation guide.

## License

[MIT](LICENSE)
