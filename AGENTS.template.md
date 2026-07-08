# Memory Server

This project has an AOT memory server at `http://localhost:5070`. Use it to store
and retrieve persistent context across coding sessions.

## Startup

The server is configured as a remote MCP server. It must be running before you can use its tools. If you get connection errors, start it with:

```bash
# Via Docker (recommended — works on Linux, macOS, and Windows)
docker compose -f docker-compose.memory.yml up -d
```

The server listens on `http://localhost:5070` and exposes MCP at `/mcp`.

## When to use

- Before starting a complex task, check memory for relevant facts
- After discovering important info (bugs, decisions, patterns, conventions), store it
- When unsure about project setup or past decisions, query memory first
- Share context between agents by saving facts under shared categories and scopes

## MCP Server

The server runs an MCP (Model Context Protocol) endpoint at `http://localhost:5070/mcp` with stateless HTTP transport. Tools are auto-discoverable via `tools/list` — no manual API docs needed.

### Available tools

| Tool | Description |
|---|---|
| `memory_list` | List facts with optional filters (category, scope, key) and pagination |
| `memory_get` | Get a single fact by ID |
| `memory_search` | Search facts by keyword in key/value fields |
| `memory_set` | Create/replace a fact (higher confidence wins on conflict) |
| `memory_update` | Update an existing fact by ID (partial update) |
| `memory_delete` | Delete a fact by ID |

### Categories

Use one of: `preference`, `fact`, `concept`, `rule`, `plan`, `goal`, `task`, `note`

### Scope convention

Use the feature or area name (e.g. `auth`, `api`, `db`, `frontend`, `project`).

## Data Model

Each memory fact follows this JSON schema:

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
