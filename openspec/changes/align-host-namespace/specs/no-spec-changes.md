## Refactoring-Only — No Spec Changes

This change aligns namespace declarations in `Mittens.Host` with the project's assembly name. It has zero impact on:

- API contracts (REST routes, request/response shapes, MCP tools)
- Database schema (table names, columns, indexes)
- Behavior (validation, conflict resolution, search, pagination)
- Configuration or deployment

No new capabilities are added, and no existing capabilities have requirement changes. This is purely a codebase consistency improvement.
