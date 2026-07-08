# Memory Server

This project has an AOT memory server at `http://localhost:5070`. Use it to store
and retrieve persistent context across coding sessions.

## Startup

The server is configured as a remote MCP server in `opencode.json`. It must be
running before you can use its tools. If you get connection errors, start it
with:

```bash
# Via Docker (recommended — works on Linux, macOS, and Windows)
docker compose up -d

# Or directly with .NET
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

## Releases

Use [semver](https://semver.org) for versioning: `vMAJOR.MINOR.PATCH`.

- **MAJOR** — breaking API or behavioral changes
- **MINOR** — new features, backward compatible
- **PATCH** — bug fixes, performance improvements, refactors

After every nontrivial change, determine the correct version bump, inspect the latest tag, and create a new one **before pushing**:

```bash
git fetch --tags
git tag v1.2.3 && git push origin v1.2.3 && git push origin main
```

The CI pipeline will build, test, and push the Docker image to Docker Hub with tags `1.2.3`, `1.2`, and `latest`.

Tag only from `main` after ensuring CI passes. **Always create the tag locally before pushing commits — pushing first then tagging breaks the release ordering.**
