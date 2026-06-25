# Memory Server

This project has an AOT memory server at `http://localhost:5070`. Use it to store
and retrieve persistent context across coding sessions.

## Startup

The server is configured as a remote MCP server in `opencode.json`. It must be
running before you can use its tools. If you get connection errors, start it
with:

```bash
dotnet run --project src/AotMemoryServer
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
