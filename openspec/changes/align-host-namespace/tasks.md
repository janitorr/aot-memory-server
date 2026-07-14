## 1. Update Project Configuration

- [x] 1.1 Change `<RootNamespace>Mittens</RootNamespace>` to `<RootNamespace>Mittens.Host</RootNamespace>` in `src/Mittens.Host/Mittens.Host.csproj`

## 2. Update Namespace Declarations in Host Source Files

- [x] 2.1 Update namespaces in `Memory/Data/` (5 files): `Mittens.Memory.Data` → `Mittens.Host.Memory.Data`
- [x] 2.2 Update namespaces in `Memory/Data/Compiled/` (3 files): `Mittens.Memory.Data.Compiled` → `Mittens.Host.Memory.Data.Compiled`
- [x] 2.3 Update namespaces in `Memory/Endpoints/` (2 files): `Mittens.Memory.Endpoints` → `Mittens.Host.Memory.Endpoints`
- [x] 2.4 Update namespaces in `Endpoints/` and `Serialization/` (3 files): `Mittens.Endpoints` → `Mittens.Host.Endpoints`, `Mittens.Serialization` → `Mittens.Host.Serialization`
- [x] 2.5 Update `using` directives in `Program.cs` to use `Mittens.Host.*` imports

## 3. Update Integration Tests

- [x] 3.1 Update `using Mittens.Memory.Data` → `using Mittens.Host.Memory.Data` in `CustomWebApplicationFactory.cs`
- [x] 3.2 Update `using Mittens.Serialization` → `using Mittens.Host.Serialization` in `RestEndpointTests.cs`

## 4. Update Documentation

- [x] 4.1 Update `AGENTS.md` naming conventions: change `root namespace Mittens` to `root namespace Mittens.Host`

## 5. Build and Verify

- [x] 5.1 Run `dotnet build` — verify no compilation errors
- [x] 5.2 Run `dotnet test` — verify all unit and integration tests pass
- [x] 5.3 Run `dotnet publish -c Release` — verify AOT build succeeds
