## Why

The `Mittens.Host` project has a mismatch between its assembly name (`Mittens.Host`) and its root namespace (`Mittens`). All source files manually declare namespaces under `Mittens.*` while the compiled binary is `Mittens.Host.dll`. The `Mittens.Core` project is consistent (both are `Mittens.Core`), making Host the odd one out. This confuses new contributors, breaks IDE navigation expectations, and violates standard .NET convention.

## What Changes

- Change `RootNamespace` in `Mittens.Host.csproj` from `Mittens` to `Mittens.Host`
- Update all namespace declarations in `src/Mittens.Host/` source files to use `Mittens.Host.*`
- Update `using` directives in `Program.cs` and integration tests to match
- Update `AGENTS.md` naming conventions section to reflect the new root namespace
- No changes to assembly name, binary output, API contracts, or database schema

## Capabilities

### New Capabilities

*(None — this is a pure refactoring with no new functionality.)*

### Modified Capabilities

*(None — namespace changes are implementation details that don't alter requirements, API contracts, or behavior.)*

## Impact

| Area | Change |
|------|--------|
| **Project file** | `<RootNamespace>Mittens</RootNamespace>` → `<RootNamespace>Mittens.Host</RootNamespace>` |
| **Source files (13)** | `Mittens.Memory.*` → `Mittens.Host.Memory.*`, `Mittens.Endpoints` → `Mittens.Host.Endpoints`, `Mittens.Serialization` → `Mittens.Host.Serialization` |
| **Program.cs** | 5 `using` directives updated |
| **Integration tests (2)** | `using` statements updated |
| **AGENTS.md** | Naming conventions section updated |
| **Build output** | Unchanged — still `Mittens.Host.dll` |
| **API surface** | No breaking changes — REST routes, MCP tools, response shapes all unchanged |
| **Database** | No changes |
