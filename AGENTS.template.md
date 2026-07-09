# Mittens

You have access to a persistent memory store at `http://localhost:5070`. Use it to store and retrieve persistent context across coding sessions.

## Session Protocol — READ THIS FIRST

### Every Session Start

Before doing anything else:

1. `mittens_search` with `scope=project`, `category=fact,rule,concept`
2. `mittens_search` with the project name or relevant area as keyword
3. **Never say "I don't know"** about the user's preferences, project setup, conventions, or past decisions without searching memory first

### Before You Store — Gate Check

Ask yourself before every `mittens_set`:

| If the fact is... | Then... |
|---|---|
| Conversational ephemera ("user asked about X at 3pm") | **SKIP** — memory is for persistent context, not chat logs |
| Already in the project's AGENTS.md or instructions | **SKIP** — it's already loaded |
| A secret, token, or password | **SKIP** — the server will reject it |
| The same fact under a different key | **SKIP** — search first to avoid duplicates |
| Missing a scope or a descriptive key | **SKIP** — unfilterable, unsearchable |
| A decision, bug pattern, convention, or preference | **STORE** — this helps future sessions |

### After Completing Work

- Store decisions, bug patterns, or conventions discovered during the task using `mittens_set`
- If the user states a preference, save it immediately

## When to Use Memory

| User says / does | Your response |
|---|---|
| "remember X" / "save this" / "store that" | `mittens_set` to persist it |
| "do you remember?" / "what do I know about X?" | `mittens_search` if unsure of scope, `mittens_list` if scope is known |
| "forget that" / "remove X" / "delete X" | `mittens_search` to find the ID, then `mittens_delete` |
| "what did we decide about X?" | `mittens_search` with the topic keyword |
| "what are the project conventions?" | `mittens_list` with `category=rule` and `scope=project` |
| Before starting a complex task | `mittens_search` for related facts in the relevant scope |

## Category Reference

| Category | Use when | Example key | Example value |
|---|---|---|---|
| `preference` | User expresses a code style, tool, or workflow preference | `coding/tab-size` | "Use 2-space indentation, not tabs" |
| `fact` | Something verifiably true about the project or codebase | `api/health-endpoint` | "Health check returns 200 at /api/health" |
| `concept` | Abstract idea, pattern, or architectural decision | `auth/stateless-tokens` | "Auth uses stateless JWT, no backend session store" |
| `rule` | A binding constraint — lint rules, deploy rules, security rules | `security/no-secrets-in-git` | "Never commit secrets, tokens, or API keys to git" |
| `plan` | A proposed approach not yet executed | `db/migrate-to-postgres` | "Plan: migrate SQLite to PostgreSQL using EF Core migrations" |
| `goal` | A desired outcome or target for the project | `perf/cold-start-under-2s` | "Target: cold start under 2 seconds on first request" |
| `task` | A concrete action item with a clear deliverable | `auth/add-middleware` | "Add JWT authentication middleware to /api routes" |
| `note` | An informal observation, reminder, or context note | `ci/flaky-macos-runner` | "macOS CI runner is flaky — tests sometimes timeout" |

## Key Naming

Use `<area>/<kebab-case-descriptor>` format: `auth/no-backend-sessions`, `db/sqlite-aot-compiled-model`, `project/deploy-port-5070`.

## Confidence

- `1.0` — confirmed by user or verified in code
- `0.8` — strong inference from observation
- `<0.5` — don't store

When two facts share the same `(category, scope, key)`, the one with higher confidence wins.

## Available Tools

| Tool | Description |
|---|---|
| `mittens_list` | List facts with optional filters (category, scope, key) and pagination |
| `mittens_get` | Get a single fact by ID |
| `mittens_search` | Search facts by keyword in key/value fields |
| `mittens_set` | Create/replace a fact (higher confidence wins on conflict) |
| `mittens_update` | Update an existing fact by ID (partial update) |
| `mittens_delete` | Delete a fact by ID |

## Scope Convention

Use the feature or area name (e.g. `auth`, `api`, `db`, `frontend`, `project`).

## Troubleshooting: Starting the Server

The server must be running before you can use its tools. If you get connection errors, start it with Docker:

```bash
# If docker-compose.memory.yml doesn't exist, create it first:
curl -O https://raw.githubusercontent.com/janitorr/mittens/main/docker-compose.example.yml
mv docker-compose.example.yml docker-compose.memory.yml

# Then start:
docker compose -f docker-compose.memory.yml up -d
```

The server listens on `http://localhost:5070` and exposes MCP at `/mcp`.
