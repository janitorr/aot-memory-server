# Plan: Document AOT Memory Server as OpenSpec Baseline Specs

## Summary

The AOT Memory Server is already fully built, but its documented behavior lives
only in `plan/` (outside OpenSpec). There is no machine-validateable "current
state" contract to diff future changes against. This change captures the
existing server as OpenSpec baseline specs across four capabilities, fills in
`openspec/config.yaml` project context, and retires `plan/` once validation
passes.

No application code changes. This is documentation/spec capture only.

## Decisions (locked with user)

1. **Four capabilities** split by layer (not a single mega-spec).
2. Specs use **correct OpenSpec format** (`## ADDED Requirements`,
   `### Requirement:`, `#### Scenario:` with WHEN/THEN, SHALL/MUST normative).
3. **`config.yaml` context block** filled in (tech stack + AOT constraints).
4. Retire `plan/` only **after** `openspec validate --specs` passes.

## Capability Specs to Create

### 1. `memory-fact-store`
Grounded in `src/AotMemoryServer/Models/MemoryFact.cs` and
`MemoryFactValidator.cs`.

- **Entity model**: `Id` (int PK), `Category`, `Key`, `Value`, `Scope`
  (required strings), `Confidence` (double, default 1.0), `Source?`,
  `UpdatedAt` (DateTimeOffset), `IsDeprecated` (bool).
- **Validation rules** (from `MemoryFactValidator.Validate`):
  - `Key` and `Value` MUST be non-empty.
  - `Value` length MUST NOT exceed 10,000 chars.
  - `Value` MUST NOT match secret regex (`sk-...`, `api_key`, `secret`,
    `token`, `password`, `private key`, `-----BEGIN ... KEY-----`) — hard error.
  - `Category` MUST be non-empty; unknown category is a **warning only**
    (allow-list: preference, fact, concept, rule, plan, goal, task, note).
- **Conflict resolution** (`ResolveConflict`): on upsert with existing
  (Category, Key, Scope), incoming wins if `incoming.Confidence >
  existing.Confidence` OR `force=true`; otherwise existing is retained.

### 2. `rest-api`
Grounded in `Endpoints/MemoryEndpoints.cs`, `HealthEndpoints.cs`,
`Abstractions/PagedResult.cs`, `Serialization/Dtos.cs`.

- `GET /api/memory` — filters `category?`, `scope?`, `key?`, `page=1`,
  `pageSize=20`; returns `PagedResult<MemoryFact>`.
- `GET /api/memory/{id:int}` — 200 fact or 404.
- `POST /api/memory` — body `MemoryFact`, `force=false` query; 200 result or
  400 (`ErrorResponse` with validation errors).
- `PUT /api/memory/{id:int}` — 200 updated, 404 if missing, 400 on validation.
- `DELETE /api/memory/{id:int}` — 204 if deleted, 404 if missing.
- `GET /api/health` — 200 `{"status":"healthy"}` or 503 on DB failure.
- Pagination: `page` clamped to >=1, `pageSize` clamped to 1..100.

### 3. `mcp-server`
Grounded in `Endpoints/MemoryMcpTools.cs`, `Program.cs` (`MapMcp`,
`WithHttpTransport(Stateless=true)`).

- Transport: `POST /mcp`, **stateless** HTTP, official ModelContextProtocol SDK
  v1.4.0.
- Six tools (all return JSON strings):
  - `MemoryList` (category?, scope?, key?, page=1, pageSize=20)
  - `MemoryGet` (id) → `MemoryFact` JSON or null
  - `MemorySearch` (q, category?, scope?, page, pageSize) — LIKE on Key/Value
  - `MemorySet` (category, key, value, scope, confidence=1.0, source?) → upsert
  - `MemoryUpdate` (id, partial fields) → update; `{"error":"Fact not found"}`
    if missing
  - `MemoryDelete` (id) → `"true"`/`"false"`
- Create/update tools enforce the same validation rules as REST.

### 4. `persistence`
Grounded in `Data/AppDbContext.cs`, `Program.cs` (inline DDL), compiled model.

- SQLite DB (conn string `DefaultDb` or `Data Source=memory.db`).
- Table `MemoryFacts` with columns matching entity; `Id` AUTOINCREMENT PK.
- Unique index `IX_MemoryFacts_Category_Key_Scope`.
- Index `IX_MemoryFacts_Scope`.
- Schema created via inline `CREATE TABLE IF NOT EXISTS` + indexes on startup
  (no EF migrations — AOT incompatible).
- AOT constraints: EF Core **compiled model** (`Data/Compiled/`), source-
  generated JSON (`AppJsonContext`), source-generated regex, raw SQL handlers.

## config.yaml context block

Add under `context:`:
- Tech stack: .NET 10, ASP.NET minimal APIs, EF Core + SQLite, ModelContextProtocol SDK v1.4.0, Scalar/OpenAPI.
- AOT requirements: PublishAot=true; no reflection-based serialization or
  migrations; use compiled model, source generators, raw SQL.
- Conventions: CQRS handlers (`IQueryHandler`/`ICommandHandler`), kebab-case
  change names, semver tags.

## Tasks

```
## 1. Config
- [ ] 1.1 Fill openspec/config.yaml context block with tech stack + AOT constraints

## 2. Baseline Specs
- [ ] 2.1 Write openspec/changes/document-baseline-specs/specs/memory-fact-store/spec.md
- [ ] 2.2 Write specs/rest-api/spec.md
- [ ] 2.3 Write specs/mcp-server/spec.md
- [ ] 2.4 Write specs/persistence/spec.md

## 3. Sync & Validate
- [ ] 3.1 openspec sync-specs --change document-baseline-specs
- [ ] 3.2 openspec validate --specs  (must pass)
- [ ] 3.3 openspec validate --changes (must pass)

## 4. Retire plan/
- [ ] 4.1 Remove plan/implementation-plan.md and plan/status.md (after validation passes)
- [ ] 4.2 openspec archive document-baseline-specs
```

## Verification

- `openspec validate --specs` → exit 0, no errors.
- `openspec validate --changes` → change validates.
- After archive, `openspec list` shows no active changes and four specs present.
- Confirm `src/` unchanged (git status clean except openspec/ + removed plan/).
