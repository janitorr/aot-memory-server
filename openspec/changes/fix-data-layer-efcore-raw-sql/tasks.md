## 1. Rewrite FactReader.cs — replace raw ADO.NET with EF Core raw SQL

- [x] 1.1 Rewrite `GetByIdAsync` to use `db.MemoryFacts.FromSqlRaw(MemoryFactSql.GetById, id)` — remove `SqliteConnection`/`SqliteCommand`/`SqliteDataReader`/`MapFact()` boilerplate
- [x] 1.2 Rewrite `GetByCategoryKeyScopeAsync` to use `db.MemoryFacts.FromSqlRaw(MemoryFactSql.GetByCategoryKeyScope, ...)` — same pattern
- [x] 1.3 Rewrite `ListAsync` to use `db.MemoryFacts.FromSqlRaw(MemoryFactSql.GetFactsPage, ...)` for entity list + `db.Database.SqlQueryRaw<int>(MemoryFactSql.GetFactsCount, ...)` for count — replace dual `SqliteCommand` approach
- [x] 1.4 Rewrite `SearchAsync` to use `db.MemoryFacts.FromSqlRaw(MemoryFactSql.SearchFactsPage, ...)` for entity list + `db.Database.SqlQueryRaw<int>(MemoryFactSql.SearchFactsCount, ...)` for count — same pattern
- [x] 1.5 Remove `MapFact(DbDataReader)` helper method — EF Core handles entity materialization from `FromSqlRaw`
- [x] 1.6 Remove `using Microsoft.Data.Sqlite`, `using System.Data`, `using System.Data.Common` from `FactReader.cs`

## 2. Rewrite FactWriter.InsertAsync — use ExecuteSqlInterpolatedAsync

- [x] 2.1 Rewrite `InsertAsync` to use `ExecuteSqlInterpolatedAsync` with the existing INSERT SQL — matching `UpdateAsync`/`DeleteAsync` pattern
- [x] 2.2 Remove `SqliteConnection`/`SqliteCommand`/`SqliteParameter` boilerplate from `InsertAsync`
- [x] 2.3 Remove `using Microsoft.Data.Sqlite` and `using System.Data` from `FactWriter.cs` (if no other references remain)

## 3. Verify AOT compilation

- [x] 3.1 Run `dotnet publish -c Release` — confirm no trimming/reflection warnings
- [x] 3.2 Confirm `FromSqlRaw` on `DbSet<MemoryFact>` works with the compiled model (`Data/Compiled/`)

## 4. Run tests

- [x] 4.1 Run unit tests (`tests/AotMemoryServer.Tests.Unit`) — all pass
- [x] 4.2 Run integration tests (`tests/AotMemoryServer.Tests.Integration`) — all pass, confirming REST/MCP parity, pagination, conflict resolution, and unique constraint behavior
- [x] 4.3 Run `openspec validate fix-data-layer-efcore-raw-sql` to confirm artifacts pass

## 5. Version bump

- [ ] 5.1 Bump version with a PATCH semver tag per AGENTS.md (e.g., `v1.0.4`) after CI passes
