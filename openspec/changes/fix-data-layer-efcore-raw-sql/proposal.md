## Why

The `persistence` spec (archived from `fix-efcore-compiled-query-runtime`) mandates `FromSqlRaw`/`SqlQueryRaw` for reads and bans `Microsoft.Data.Sqlite` `SqliteCommand` usage, but the data layer was never fully migrated. `FactReader.cs` (120 lines) still uses raw ADO.NET (`SqliteConnection`, `SqliteCommand`, manual `SqliteParameter` wiring, `DbDataReader` → `MemoryFact` mapping). `FactWriter.InsertAsync` also uses raw ADO.NET. This inconsistency leaves the codebase in a half-migrated state where the spec says one thing and the code does another.

## What Changes

- Rewrite `Data/FactReader.cs` to use `FromSqlRaw` on `DbSet<MemoryFact>` for entity reads and `SqlQueryRaw<int>` for counts — no more `SqliteConnection`/`SqliteCommand`/`SqliteDataReader`/`MapFact()`
- Rewrite `Data/FactWriter.InsertAsync` to use `ExecuteSqlInterpolatedAsync` — matching `UpdateAsync` and `DeleteAsync` which already use it
- Remove `MapFact()` helper — EF Core handles entity materialization from `FromSqlRaw`
- Remove `Microsoft.Data.Sqlite` usings from `Data/` directory
- `MemoryFactSql.cs` SQL constants remain unchanged (already parameterized with `@p` placeholders)
- No REST/MCP contract changes, no handler-level changes, no model changes

## Capabilities

### New Capabilities
<!-- none — this change is internal refactoring, no new externally-visible capability -->

### Modified Capabilities
<!-- none — the persistence spec already mandates FromSqlRaw/SqlQueryRaw; this change implements what the spec already requires -->

## Impact

- **Affected files**: `src/AotMemoryServer/Data/FactReader.cs` (rewrite), `src/AotMemoryServer/Data/FactWriter.cs` (partial rewrite — InsertAsync only)
- **Dependencies**: unchanged (EF Core 10 + SQLite)
- **Behavior**: identical external behavior (REST + MCP contracts unchanged). Internal data access moves from manual ADO.NET to EF Core raw SQL.
- **Risk**: `FromSqlRaw` on `DbSet<MemoryFact>` must compose correctly with the AOT compiled model — verify with `dotnet publish -c Release` and full test suite. The entity is already scaffolded in `Data/Compiled/` so the compiled model knows its shape.
