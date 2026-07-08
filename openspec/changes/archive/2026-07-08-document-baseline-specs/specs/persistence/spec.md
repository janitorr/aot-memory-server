## ADDED Requirements

### Requirement: SQLite storage
The system SHALL persist memory facts in a SQLite database (connection string from DefaultDb or default "Data Source=memory.db").

#### Scenario: Database file created
- **WHEN** application starts with default connection string
- **THEN** memory.db file is created if not exists

### Requirement: MemoryFacts table schema
The system SHALL create a table MemoryFacts with columns: Id (INTEGER PRIMARY KEY AUTOINCREMENT), Category (TEXT NOT NULL), Key (TEXT NOT NULL), Value (TEXT NOT NULL), Scope (TEXT NOT NULL), Confidence (REAL NOT NULL), Source (TEXT NULL), UpdatedAt (TEXT NOT NULL), IsDeprecated (INTEGER NOT NULL).

#### Scenario: Table created on startup
- **WHEN** application starts
- **THEN** MemoryFacts table is created if not exists via inline DDL

### Requirement: Unique index on Category/Key/Scope
The system SHALL enforce uniqueness of (Category, Key, Scope) via a unique index.

#### Scenario: Duplicate insert prevented
- **WHEN** inserting a fact with duplicate Category/Key/Scope
- **THEN** database constraint prevents duplicate (handled at application level by upsert logic)

### Requirement: Index on Scope
The system SHALL create an index on Scope for efficient filtering.

#### Scenario: Scope filter uses index
- **WHEN** querying with Scope filter
- **THEN** database uses IX_MemoryFacts_Scope index

### Requirement: No EF migrations (AOT constraint)
The system SHALL NOT use EF Core migrations. Schema SHALL be created via inline SQL on startup.

#### Scenario: Startup applies DDL
- **WHEN** application starts
- **THEN** inline CREATE TABLE IF NOT EXISTS and CREATE INDEX IF NOT EXISTS are executed

### Requirement: EF Core compiled model
The system SHALL use EF Core compiled model (Data/Compiled/) for AOT compatibility.

#### Scenario: Compiled model registered
- **WHEN** DbContext is configured
- **THEN** UseModel(AppDbContextModel.Instance) is called

### Requirement: Source-generated JSON serialization
The system SHALL use System.Text.Json source generation (AppJsonContext) for AOT-safe serialization.

#### Scenario: JSON serialization uses source generator
- **WHEN** serializing MemoryFact or PagedResult
- **THEN** AppJsonContext.Default is used as TypeInfoResolver

### Requirement: Source-generated regex
The system SHALL use source-generated regex (GeneratedRegex attribute) for secret detection.

#### Scenario: Secret pattern compiled at build time
- **WHEN** SecretPattern regex is used
- **THEN** pattern is compiled into the assembly via source generation

### Requirement: Raw SQL in handlers (no LINQ query translation)
The system SHALL use raw SQL commands in CQRS handlers for AOT compatibility (no EF Core query pipeline).

#### Scenario: Handlers execute raw SQL
- **WHEN** GetFactsHandler, SearchFactsHandler, UpsertFactHandler, UpdateFactHandler, DeleteFactHandler execute
- **THEN** they use SqliteCommand with parameterized queries directly on the connection