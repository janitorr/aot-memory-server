# Mittens

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com)
[![.NET](https://github.com/janitorr/mittens/actions/workflows/dotnet.yml/badge.svg)](https://github.com/janitorr/mittens/actions/workflows/dotnet.yml)

**Agent Ledger** — persistent structured memory for AI agents.

LLMs reset between sessions. Mittens gives them a ledger — a persistent, queryable memory store that survives context windows, accessible via MCP and REST.

## How It Works

```
Agent ──write──▶ ┌──────────────────┐
                 │    M I T T E N S  │
                 │  ┌────┬────┬────┐ │
                 │  │cat │key │val │ │
                 │  ├────┼────┼────┤ │
                 │  │fact│api │200 │ │
                 │  │rule│sec │no  │ │
                 │  └────┴────┴────┘ │
                 │   SQLite (disk)   │
                 └──────────────────┘
Agent ◀──read───  MCP / REST API
```

Every fact is a ledger entry with a category, key, value, scope, and confidence. Higher confidence wins on conflict.

## Features

- **Single binary** — ~33 MB, instant startup, zero dependencies
- **REST API** — full CRUD at `/api/memory`
- **MCP endpoint** — six tools at `/mcp` (`mittens_list`, `mittens_get`, `mittens_search`, `mittens_set`, `mittens_update`, `mittens_delete`)
- **Health check** — `/api/health` with database connectivity verification
- **OpenAPI + Scalar UI** — interactive docs at `/scalar/v1`
- **Validation** — input validation, secret detection, conflict resolution
- **Search** — full-text search across keys and values
- **Pagination & filtering** — by category, scope, and key
- **CQRS via Mediator** — source-generated commands and queries
- **59 tests** — 33 unit + 26 integration

## Quick Start

```bash
# Run the server
dotnet run --project src/Mittens

# Run tests
dotnet test

# Publish a native binary
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

The container stores the SQLite database in a named volume (`mittens-data`), so data persists across restarts. Works on Linux, macOS, and Windows (Docker Desktop).

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
| POST | `/mcp` | MCP (tools: `mittens_list`, `mittens_get`, `mittens_search`, `mittens_set`, `mittens_update`, `mittens_delete`) |

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
src/Mittens/
├── Application/          # CQRS handlers, serialization, DTOs
│   ├── Abstractions/     # Shared types (PagedResult, ValidationException)
│   ├── Commands/         # Upsert, Update, Delete
│   ├── Queries/          # GetFacts, GetFactById, SearchFacts
│   └── Serialization/    # JSON serialization context and DTOs
├── Data/                 # EF Core DbContext and compiled model
│   └── Compiled/         # Precompiled EF Core model
├── Endpoints/            # REST, Health, MCP endpoint definitions
├── Models/               # MittensFact entity, validator, validation errors
└── Program.cs            # Entry point, DI, middleware, routing

tests/
├── Mittens.Tests.Unit/        # Validator & conflict resolution tests
└── Mittens.Tests.Integration/  # REST, MCP, health endpoint tests
```

## Development

```bash
# Build
dotnet build

# Run unit tests
dotnet test tests/Mittens.Tests.Unit

# Run integration tests
dotnet test tests/Mittens.Tests.Integration
```

## Using with opencode

### 1. Run the server

Copy [`docker-compose.example.yml`](docker-compose.example.yml) into your project and start it:

```bash
docker compose -f docker-compose.example.yml up -d
```

Or download it directly:

```bash
curl -O https://raw.githubusercontent.com/janitorr/mittens/main/docker-compose.example.yml
docker compose -f docker-compose.example.yml up -d
```

The server listens at `http://localhost:5070`.

### 2. Configure opencode (global)

Add the memory server to your global OpenCode configuration at `~/.config/opencode/opencode.json`:

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

If the file does not exist, create it. If it already has other MCP servers, add the `memory` entry alongside them.

### 3. Add agent instructions (global)

Download [`AGENTS.template.md`](AGENTS.template.md) as `memory-server.md` in your global OpenCode configuration directory:

```bash
mkdir -p ~/.config/opencode
curl -o ~/.config/opencode/memory-server.md https://raw.githubusercontent.com/janitorr/mittens/main/AGENTS.template.md
```

Then add `"memory-server.md"` to the `instructions` array in `~/.config/opencode/opencode.json`:

```json
{
  "instructions": ["memory-server.md"]
}
```

If `instructions` already exists with other entries, append `memory-server.md` to the array. OpenCode resolves instruction paths relative to the config file directory.

For a complete step-by-step install and uninstall guide (including merge guidance for existing configs), see [`SETUP.md`](SETUP.md).

### Agent setup instructions

If you are an AI agent helping a user install this memory server, read [`SETUP.md`](SETUP.md) for the complete step-by-step installation guide.

## License

[MIT](LICENSE)

*Named after a cat that never existed.*
