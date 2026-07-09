## ADDED Requirements

### Requirement: No raw ADO.NET in Data layer
The system SHALL NOT use `Microsoft.Data.Sqlite` `SqliteConnection`, `SqliteCommand`, or `SqliteDataReader` in the `Data/` directory. All data access SHALL go through EF Core's `FromSqlRaw`, `SqlQueryRaw`, or `ExecuteSqlInterpolatedAsync` APIs.

#### Scenario: Data layer uses EF Core raw SQL only
- **WHEN** FactReader or FactWriter executes any data access operation
- **THEN** they use `FromSqlRaw`, `SqlQueryRaw`, or `ExecuteSqlInterpolatedAsync` — no raw ADO.NET

#### Scenario: No Microsoft.Data.Sqlite in Data directory
- **WHEN** the `Data/` directory is inspected
- **THEN** no files contain `using Microsoft.Data.Sqlite` or `SqliteConnection`/`SqliteCommand`/`SqliteDataReader`
