## Context

The `persistence` spec (archived from `fix-efcore-compiled-query-runtime`) mandates `FromSqlRaw`/`SqlQueryRaw` for reads and bans `Microsoft.Data.Sqlite` `SqliteCommand` usage. However, the data layer was never fully migrated:

- `FactReader.cs` (120 lines) uses raw ADO.NET: `SqliteConnection`, `SqliteCommand`, manual `SqliteParameter` wiring, `DbDataReader` → `MemoryFact` mapping via `MapFact()` helper
- `FactWriter.InsertAsync` uses raw ADO.NET: `conn.CreateCommand()`, manual `SqliteParameter` wiring, `ExecuteScalarAsync` for last insert ID
- `FactWriter.UpdateAsync` and `DeleteAsync` already use `ExecuteSqlInterpolatedAsync` — the only consistent EF Core usage

The handlers (`Application/Queries/*`, `Application/Commands/*`) are clean — they call `FactReader.ListAsync(...)`, `FactWriter.UpdateAsync(...)` with no knowledge of the underlying data access mechanism.

## Goals / Non-Goals

**Goals:**
- Rewrite `FactReader` to use `FromSqlRaw` on `DbSet<MemoryFact>` for entity reads and `SqlQueryRaw<int>` for counts
- Rewrite `FactWriter.InsertAsync` to use `ExecuteSqlInterpolatedAsync` — matching the pattern already used by `UpdateAsync` and `DeleteAsync`
- Remove `MapFact()` helper — EF Core handles entity materialization
- Remove all `Microsoft.Data.Sqlite` usings from the `Data/` directory
- Keep `MemoryFactSql.cs` SQL constants unchanged (already parameterized with `@p` placeholders)

**Non-Goals:**
- No spec changes — the `persistence` spec already mandates the right approach
- No handler changes — handler signatures and behavior remain identical
- No REST/MCP contract changes — external behavior is unchanged
- No model changes — `MemoryFact` entity and `AppDbContext` remain the same

## Decisions

### Decision: Use `FromSqlRaw` on `DbSet<MemoryFact>` for reads

`FromSqlRaw` on `DbSet<T>` executes raw SQL and maps results to entities using the compiled model. This is AOT-safe because:
- No LINQ query translation at runtime — the SQL is already fully formed
- The compiled model (`Data/Compiled/`) pre-computes entity metadata at build time
- `FromSqlRaw` with no `.Where()`/`.Select()` chains just maps columns to properties

Alternative considered: `db.Database.SqlQueryRaw<MemoryFact>(...)` — this also works but bypasses the `DbSet` and its compiled model registration. Using `FromSqlRaw` on `DbSet<MemoryFact>` is more idiomatic and keeps the entity within its normal EF Core context.

### Decision: Use `SqlQueryRaw<int>` for count queries

Count queries return a scalar value, not an entity. `SqlQueryRaw<int>` is the appropriate API — it executes raw SQL and returns the result as a strongly-typed value. This avoids the overhead of entity materialization for a simple integer.

### Decision: Use `ExecuteSqlInterpolatedAsync` for InsertAsync

`ExecuteSqlInterpolatedAsync` takes a `FormattableString` parameter, which the C# compiler converts to a parameterized SQL command (not string interpolation). This matches the pattern already used by `UpdateAsync` and `DeleteAsync`, creating consistency across all write operations.

Alternative considered: `ExecuteSqlRawAsync` with manual `SqliteParameter` array — more verbose but equivalent. `ExecuteSqlInterpolatedAsync` is cleaner and less error-prone.

### Decision: Keep `MemoryFactSql.cs` SQL constants

The SQL constants in `MemoryFactSql.cs` are already parameterized with `@p0`, `@p1`, etc. They work equally well with `FromSqlRaw`/`SqlQueryRaw` as they did with raw ADO.NET. No changes needed.

## Risks / Trade-offs

| Risk | Mitigation |
|---|---|
| `FromSqlRaw` on `DbSet<MemoryFact>` may not work with AOT compiled model | Verify with `dotnet publish -c Release` — if it fails, fall back to `SqlQueryRaw<MemoryFact>` which bypasses `DbSet` |
| `FromSqlRaw` may trigger runtime LINQ translation | Ensure no `.Where()`/`.Select()` chains are appended — `FromSqlRaw` with no further LINQ is just raw SQL execution |
| `ExecuteSqlInterpolatedAsync` may have different parameter naming than raw ADO.NET | EF Core handles parameter naming automatically — no manual `@p0` wiring needed |
| Entity materialization may differ from manual `MapFact()` | Run full test suite — if any field mapping breaks, tests will catch it |
