## Context

The project is currently named "AOT Memory Server" with the namespace `AotMemoryServer`, Docker image `janitorr/aot-memory-server`, and project directory `src/AotMemoryServer`. The name embeds a .NET implementation detail (AOT compilation) into the product identity. The project needs to be renamed to "Mittens" with the tagline "Agent Ledger" across all code, documentation, CI, and infrastructure.

Current state:
- Source: `src/AotMemoryServer/`
- Namespaces: `AotMemoryServer.*`
- Docker: `janitorr/aot-memory-server`
- GitHub: `janitorr/aot-memory-server`
- Database table: `MemoryFacts`
- MCP tools: `MemoryList`, `MemoryGet`, `MemorySearch`, `MemorySet`, `MemoryUpdate`, `MemoryDelete`
- Binary: `AotMemoryServer` (native entrypoint)
- Docker Compose: service `memory-server`, container `aot-memory-server`, volume `memory-data`

## Goals / Non-Goals

**Goals:**
- Rename all project identity references from AOT Memory Server / aot-memory-server / AotMemoryServer to Mittens / mittens / Mittens
- Maintain full functional parity — no behavior changes
- Ensure the project builds, tests pass, and Docker image publishes under the new name
- Update all documentation and agent instructions

**Non-Goals:**
- No functional changes or new features
- No database schema migration strategy (the table rename is handled in the rename — existing users will need fresh DB or manual migration)
- No changes to MCP protocol behavior, REST API structure, or HTTP routes (only names change)

## Decisions

### Decision 1: Namespace is `Mittens`, not `MittensServer` or `MittensAgentLedger`
**Rationale:** The binary name is `Mittens`, the project name is `Mittens`. The namespace should match. Short, clean, no qualifier needed. The tagline "Agent Ledger" is descriptive but not part of the namespace.

### Decision 2: Docker image is `janitorr/mittens`, not `janitorr/mittens-agent-ledger`
**Rationale:** Docker Hub names should be short and memorable. "Mittens" is distinctive enough to be searchable. The tagline belongs in README and Docker Hub description, not the image name.

### Decision 3: MCP tool names use `mittens_` prefix
**Rationale:** MCP tool names should be namespaced to avoid collisions with other MCP servers. `mittens_list`, `mittens_set`, etc. follow the same pattern as the old `memory_*` convention but with the new project identity.

### Decision 4: Database table is `MittensFacts`
**Rationale:** Consistent with the namespace. The table name change is a breaking change for existing databases. Since this is a memory store (not critical production data), users can simply start fresh with a new database.

### Decision 5: Docker Compose uses `mittens` for service and container names
**Rationale:** Consistent with the project name. Volume name changes from `memory-data` to `mittens-data`.

### Decision 6: Binary name is `Mittens` (capital M)
**Rationale:** The assembly name and output binary follow the namespace convention. On Linux/macOS this produces `./Mittens`, on Windows `Mittens.exe`.

### Alternatives Considered

| Option | Pros | Cons |
|---|---|---|
| Keep `AotMemoryServer` namespace, only rename externally | Less code churn | Inconsistent; namespace still leaks tech detail |
| Rename to `AgentLedger` namespace | Functional name | Loses the "Mittens" brand identity |
| Keep `MemoryFacts` table name | Backward compatible | Inconsistent with new naming convention |

## Risks / Trade-offs

**[Risk] Existing users break** → Users with `janitorr/aot-memory-server` pulled will need to update their docker-compose files. Mitigation: Clear migration notes in README.

**[Risk] Database incompatibility** → Existing `memory.db` files contain `MemoryFacts` table; new code expects `MittensFacts`. Mitigation: Document that existing users need to delete the old database or manually rename the table. Since this is agent memory (not critical data), fresh start is acceptable.

**[Risk] GitHub/Docker Hub rename is manual** → Requires owner action on GitHub and Docker Hub. Mitigation: Tasks are clearly marked for manual execution.

**[Risk] MCP client breakage** → Agents that have `memory_*` tool names hardcoded will fail. Mitigation: This is a breaking change; version bump to MAJOR.

**[Trade-off] Atomic rename vs phased** → An atomic rename (everything in one commit) is cleaner but riskier. A phased approach (rename code first, docs later) reduces risk but leaves the project in an inconsistent state temporarily. Decision: atomic rename — one commit, all changes together.

## Migration Plan

1. Rename all code (namespaces, classes, project names)
2. Rename all documentation
3. Update Docker and CI configuration
4. Update database table name in DDL
5. Build, test, verify
6. Create single commit with all changes
7. **Manual**: Rename GitHub repository to `mittens`
8. **Manual**: Create Docker Hub repository `janitorr/mittens`
9. Tag new MAJOR version (e.g., `v2.0.0`)
10. CI builds and publishes to `janitorr/mittens`

**Rollback:** Revert the rename commit. GitHub repo and Docker Hub repo would need manual revert.

## Open Questions

- Should we keep `AotMemoryServer` as a legacy namespace alias for backward compatibility? (Decision: no, this is a MAJOR version bump — clean break)
- Should the old Docker image `janitorr/aot-memory-server` be deprecated with a notice? (Decision: yes, add deprecation note to old repo if possible)
