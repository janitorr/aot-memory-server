# Memory Server

This project has an AOT memory server at `http://localhost:5070`. Use it to store
and retrieve persistent context across coding sessions.

## Startup

The server must be running before you can use its tools. If you get connection errors, start it with Docker:

```bash
# If docker-compose.memory.yml doesn't exist, create it first:
curl -O https://raw.githubusercontent.com/janitorr/aot-memory-server/main/docker-compose.example.yml
mv docker-compose.example.yml docker-compose.memory.yml

# Then start:
docker compose -f docker-compose.memory.yml up -d
```

The server listens on `http://localhost:5070` and exposes MCP at `/mcp`.

## When to Use Memory

### Explicit Triggers

| User says / does | Your response |
|---|---|
| "remember X" / "save this" / "store that" | `memory_set` to persist it |
| "do you remember?" / "what do I know about X?" | `memory_search` if unsure of scope, `memory_list` if scope is known |
| "forget that" / "remove X" / "delete X" | `memory_search` to find the ID, then `memory_delete` |
| "what did we decide about X?" | `memory_search` with the topic keyword |
| "what are the project conventions?" | `memory_list` with `category=rule` and `scope=project` |
| Before starting a complex task | `memory_search` for related facts in the relevant scope |
| After fixing a tricky bug | `memory_set` with the bug pattern so future sessions know |
| User states a preference | `memory_set` with category `preference` |

### Retrieval Protocol

| Phase | What to do |
|---|---|
| **Session start** | `memory_search` for project-wide facts (`scope=project`, `category=fact,rule,concept`) |
| **Before investigating** | Search area-specific facts (`scope=<relevant-area>`) |
| **Mid-task discovery** | Check if fact already exists (`memory_search`) before storing — avoid duplicates |
| **After completing work** | Store decisions, patterns, or conventions discovered during the task |

## Category Decision Tree

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

## Key Naming Convention

Use `<area>/<kebab-case-descriptor>` format:

| Good | Why |
|---|---|
| `auth/no-backend-sessions` | Clear area + descriptive name |
| `db/sqlite-aot-compiled-model` | Specific and searchable |
| `project/deploy-port-5070` | Project-level fact with concrete detail |
| `perf/cold-start-under-2s` | Category matches key intent |

| Bad | Why |
|---|---|
| `thing1` | Unsearchable, meaningless |
| `auth-thing` | No area separator, vague |
| `the-bug-we-found` | Describes nothing |
| `note-about-something` | Too generic to retrieve |

## Confidence Semantics

| Confidence | Meaning | When to use |
|---|---|---|
| `1.0` | Settled fact — confirmed by user or verified in code | User explicitly told you this, or you read it in source |
| `0.7–0.9` | Strong inference from observation | You read it in source but no explicit user confirmation |
| `0.5–0.6` | Tentative observation | You noticed it once, could be wrong — mark for review |
| `<0.5` | Don't store | Too uncertain to be useful |

**Conflict resolution:** When two facts share the same `(category, scope, key)`, the one with higher `confidence` wins. Use this to update stale facts — set a new fact with higher confidence to overwrite.

## Anti-patterns

| Don't | Why |
|---|---|
| Store ephemeral conversation state ("user asked about X at 3pm") | Memory is for persistent context, not chat logs |
| Duplicate AGENTS.md content as memory facts | AGENTS.md is already loaded — don't store it in memory |
| Store secrets, tokens, or passwords | The server has secret detection that will reject these |
| Store unstructured blobs without a meaningful key | Makes retrieval impossible — always use descriptive keys |
| Store the same fact under multiple keys | Causes confusion on retrieval — search first, then update |
| Store facts without a scope | Scope is essential for filtering — always set it |

## Available Tools

| Tool | Description |
|---|---|
| `memory_list` | List facts with optional filters (category, scope, key) and pagination |
| `memory_get` | Get a single fact by ID |
| `memory_search` | Search facts by keyword in key/value fields |
| `memory_set` | Create/replace a fact (higher confidence wins on conflict) |
| `memory_update` | Update an existing fact by ID (partial update) |
| `memory_delete` | Delete a fact by ID |

## Scope Convention

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
