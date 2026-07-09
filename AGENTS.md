# Mittens

This project has an Mittens at `http://localhost:5070`. Use it to store and retrieve persistent context across coding sessions.

## Startup

The server is configured as a remote MCP server in `opencode.json`. It must be running before you can use its tools. If you get connection errors, start it with:

```bash
# Via Docker (recommended — works on Linux, macOS, and Windows)
docker compose up -d

# Or directly with .NET
dotnet run --project src/Mittens
```

The server listens on `http://localhost:5070` and exposes MCP at `/mcp`.

**Global config requirement:** Agent instructions only load if `~/.config/opencode/AGENTS.md` exists and `~/.config/opencode/opencode.json` registers `"instructions": ["mittens.md"]` (relative path). See SETUP.md.

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

## Spec Kit Workflow

This project uses OpenSpec with the `spec-driven` schema. Changes follow this flow:

1. **Propose** — Write a proposal with why, what changes, and impact
2. **Spec** — Write formal requirements with scenarios (WHEN/THEN format)
3. **Design** — Document decisions, trade-offs, and alternatives
4. **Tasks** — Break work into numbered, checkboxed steps
5. **Implement** — Work through tasks, mark complete as you go
6. **Validate** — Run `openspec validate <change-name>` to confirm artifacts pass
7. **Archive** — Run `openspec archive <change-name>` to merge into main specs

Use `/opsx-propose <change-name>` to start, `/opsx-continue` to iterate on artifacts, and `/opsx-apply` to implement.

## Releases

Use [semver](https://semver.org) for versioning: `vMAJOR.MINOR.PATCH`.

| Change type | Bump | Examples |
|---|---|---|
| External contract change | **MAJOR** | REST API, MCP protocol, data format, CLI flags |
| Behavior change | **MINOR** | New features, internal logic changes |
| Implementation-only code change | **PATCH** | Bug fixes, refactors, performance improvements |
| Specs or documentation only | **skip** | — |

Push commits and tag together:

```bash
git fetch --tags
git tag v1.2.3 && git push origin v1.2.3 && git push origin main
```

The CI pipeline will build, test, and push the Docker image to Docker Hub with tags `1.2.3`, `1.2`, and `latest`.

Tag only from `main`. If CI fails, fix the issue and create a new tag — never move or delete a pushed tag.
