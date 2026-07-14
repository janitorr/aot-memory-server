## Context

The `Mittens.Host` project defines `<AssemblyName>Mittens.Host</AssemblyName>` but `<RootNamespace>Mittens</RootNamespace>`, while the `Mittens.Core` sibling project has both set to `Mittens.Core`. All 13 source files in Host explicitly declare namespaces under `Mittens.*` rather than inheriting the root. This mismatch is cosmetic but confusing — the binary is `Mittens.Host.dll` yet code lives under `Mittens.*`, and `dotnet format` / IDE add-file templates produce inconsistent defaults.

## Goals / Non-Goals

**Goals:**
- `RootNamespace` matches `AssemblyName` (both `Mittens.Host`)
- All source files use `Mittens.Host.*` namespace root
- Build and all tests pass without errors or warnings
- AOT release build succeeds

**Non-Goals:**
- No changes to binary output name — remains `Mittens.Host.dll`
- No changes to Core project namespaces
- No API contract, database schema, or behavioral changes
- No restructuring of folders or files

## Decisions

### 1. Namespace mapping

| Old | New |
|-----|-----|
| `Mittens.Memory.Data` | `Mittens.Host.Memory.Data` |
| `Mittens.Memory.Data.Compiled` | `Mittens.Host.Memory.Data.Compiled` |
| `Mittens.Memory.Endpoints` | `Mittens.Host.Memory.Endpoints` |
| `Mittens.Endpoints` | `Mittens.Host.Endpoints` |
| `Mittens.Serialization` | `Mittens.Host.Serialization` |

### 2. File-scoped namespace declarations stay as-is

**Rationale:** All Host files already use file-scoped namespace declarations (`namespace X.Y;`). No need to change style — only the namespace value changes. The new `RootNamespace` will match for any future generated files.

### 3. No spec files needed

**Rationale:** The spec-driven schema requires specs only when behavioral requirements change. Namespace alignment is purely an implementation detail — REST routes, MCP tool names, API contracts, and database schema are all unchanged. No spec deltas are needed.

### 4. Skip specs artifact

**Rationale:** Since no capabilities are created or modified, there are no spec-level changes. The `specs` artifact will be left empty (no files matching `specs/**/*.md` are needed), which the CLI should accept as complete.

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| **Compiled EF model namespace mismatch** — generated code in `Data/Compiled/*.cs` has hardcoded namespace strings | Update namespace declarations in all 3 compiled model files alongside other source files |
| **Missed a file** — a namespace reference could be overlooked | Build will catch every miss as a compiler error |
| **Test namespace references broken** — integration tests `using` Host namespaces | Updated in lockstep; tests verify correctness |
| **AOT trim warnings introduced** | Build in Release config to verify; namespace changes shouldn't affect trimming |

## Migration Plan

1. Change `<RootNamespace>` in `Mittens.Host.csproj`
2. Update namespace declarations in all 13 Host source files
3. Update `using` directives in `Program.cs`
4. Update `using` directives in integration tests (2 files)
5. Update `AGENTS.md` naming conventions section
6. `dotnet build` — verify compilation
7. `dotnet test` — verify all tests pass
8. `dotnet publish -c Release` — verify AOT build

**Rollback:** Single git revert. No database or API changes.
