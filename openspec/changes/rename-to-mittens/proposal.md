## Why

"AOT" is an implementation detail (ahead-of-time .NET compilation), not what the project does. The current name leaks a tech choice into the product identity. "Mittens" is a distinctive, zero-tech-baggage name that emerged organically from the development process, paired with the functional tagline "Agent Ledger" to make the project's purpose immediately clear.

## What Changes

- Rename the project from "AOT Memory Server" to "Mittens"
- Tagline: "Agent Ledger" — a structured, trustworthy memory system for AI agents
- Update all namespaces, assembly names, and project names in C# code
- Update Docker image name from `janitorr/aot-memory-server` to `janitorr/mittens`
- Update repository references, documentation, and agent instructions
- Update configuration keys, Docker service names, and volume names
- Update CI workflow references
- **BREAKING**: Docker image name changes; existing deployments must pull the new image
- **BREAKING**: All namespace imports change; external consumers must update their references
- **BREAKING**: MCP tool names (where `Memory` prefix appears) change to `Mittens` convention

## Capabilities

### New Capabilities
- `project-identity`: Establish Mittens as the project name with "Agent Ledger" tagline; define naming conventions for namespaces, Docker image, and CLI binary

### Modified Capabilities
None. This is a pure rename — no functional requirements or behavioral specs change.

## Impact

- All `.cs` files: namespace renames (`AotMemoryServer` → `Mittens`)
- All `.csproj` files: assembly name, root namespace
- `Dockerfile`: ENTRYPOINT binary name, labels
- `docker-compose.yml` + `docker-compose.example.yml`: image, service, container, volume names
- `README.md`, `SETUP.md`, `AGENTS.md`, `AGENTS.template.md`: all references to project name
- `.github/workflows/dotnet.yml`: image tags, build labels
- Database: schema name, table name `MemoryFacts` → rename consideration
- MCP tool methods: `Memory*` prefix → new convention
- Git/GitHub: repo rename on GitHub (manual)
- Docker Hub: new repository name (manual)
