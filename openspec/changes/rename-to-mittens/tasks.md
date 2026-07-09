## 1. Source Code — Namespaces and Project Names

- [x] 1.1 Rename `src/AotMemoryServer/` directory to `src/Mittens/`
- [x] 1.2 Rename `src/AotMemoryServer/AotMemoryServer.csproj` to `src/Mittens/Mittens.csproj` and update AssemblyName, RootNamespace, and InternalsVisibleTo
- [x] 1.3 Replace all `namespace AotMemoryServer` with `namespace Mittens` in all `.cs` files under `src/`
- [x] 1.4 Replace all `using AotMemoryServer` with `using Mittens` in all `.cs` files under `src/`
- [x] 1.5 Rename `AotMemoryServer.slnx` to `Mittens.slnx` and update all project paths inside
- [x] 1.6 Update Dockerfile ENTRYPOINT from `./AotMemoryServer` to `./Mittens` and COPY paths

## 2. Test Projects — Namespaces and References

- [x] 2.1 Rename `tests/AotMemoryServer.Tests.Unit/` to `tests/Mittens.Tests.Unit/` and update `.csproj` name, namespace, and project reference
- [x] 2.2 Rename `tests/AotMemoryServer.Tests.Integration/` to `tests/Mittens.Tests.Integration/` and update `.csproj` name, namespace, and project reference
- [x] 2.3 Replace all `namespace AotMemoryServer.Tests.Unit` with `namespace Mittens.Tests.Unit` in unit test files
- [x] 2.4 Replace all `namespace AotMemoryServer.Tests.Integration` with `namespace Mittens.Tests.Integration` in integration test files
- [x] 2.5 Replace all `using AotMemoryServer` with `using Mittens` in test files

## 3. MCP Tools — Tool Names and Method Prefixes

- [x] 3.1 Rename MCP tool names from `memory_*` to `mittens_*` in the MCP tools registration
- [x] 3.2 Rename `MemoryMcpTools` class to `MittensMcpTools`
- [x] 3.3 Rename method prefixes from `MemoryList`, `MemoryGet`, etc. to `MittensList`, `MittensGet`, etc.

## 4. Database — Table and Schema Names

- [x] 4.1 Rename table from `MemoryFacts` to `MittensFacts` in Program.cs DDL
- [x] 4.2 Update schema name from `mem` to `mittens` in AppDbContext (if applicable)
- [x] 4.3 Rename `MemoryFact` entity class to `MittensFact`
- [x] 4.4 Update `AppDbContext.DbSet<MemoryFact>` to `DbSet<MittensFact>`
- [x] 4.5 Update all references to `MemoryFact` in queries, commands, validators, and serialization

## 5. Docker and Compose — Image and Service Names

- [x] 5.1 Update `docker-compose.yml`: image to `janitorr/mittens:latest`, service name to `mittens`, container name to `mittens`, volume to `mittens-data`
- [x] 5.2 Update `docker-compose.example.yml`: same changes as above
- [x] 5.3 Update Dockerfile labels and metadata to reference "Mittens"

## 6. CI/CD — Workflow Updates

- [x] 6.1 Update `.github/workflows/dotnet.yml`: test project paths, Docker image name from `janitorr/aot-memory-server` to `janitorr/mittens`
- [x] 6.2 Update any GitHub Actions references to old repository name

## 7. Documentation — All References

- [x] 7.1 Update `README.md`: title, tagline, all code examples, URLs, and references
- [x] 7.2 Update `SETUP.md`: title, all references to project name, Docker image, URLs, and commands
- [x] 7.3 Update `AGENTS.md`: project path references and server startup commands
- [x] 7.4 Update `AGENTS.template.md`: all URLs and references
- [x] 7.5 Update `.gitignore` comment referencing "AOT Memory Server"

## 8. Build and Verify

- [x] 8.1 Run `dotnet build` and verify compilation succeeds
- [x] 8.2 Run `dotnet test` and verify all tests pass
- [x] 8.3 Run `dotnet publish -c Release` and verify AOT binary is named `Mittens`
- [x] 8.4 Verify Docker Compose starts successfully with new service names
- [x] 8.5 Verify health endpoint responds at `http://localhost:5070/api/health`
- [x] 8.6 Verify MCP endpoint responds and tools list shows `mittens_*` prefix

## 9. Git and Release

- [ ] 9.1 Create single commit with all rename changes
- [ ] 9.2 Run `openspec validate rename-to-mittens` and fix any issues
- [ ] 9.3 Tag and push as MAJOR version (e.g., `v2.0.0`)

## 10. External — Manual Actions (Requires Owner Access)

- [ ] 10.1 **Rename GitHub repository from `aot-memory-server` to `mittens`**
- [ ] 10.2 **Create new Docker Hub repository `janitorr/mittens`**
- [ ] 10.3 **Add deprecation notice to old Docker Hub repository `janitorr/aot-memory-server`**
- [ ] 10.4 **Update any external links, badges, or references pointing to the old repository URL**
