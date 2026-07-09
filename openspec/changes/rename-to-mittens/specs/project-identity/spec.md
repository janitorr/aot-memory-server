## ADDED Requirements

### Requirement: Project name is "Mittens" with tagline "Agent Ledger"
All user-facing references to the project SHALL use the name "Mittens" with the tagline "Agent Ledger". The previous name "AOT Memory Server" SHALL be removed from all documentation, UI, and configuration.

#### Scenario: README displays new project identity
- **WHEN** a user visits the repository README
- **THEN** the title reads "Mittens" and the tagline reads "Agent Ledger"

#### Scenario: Docker Hub displays new project identity
- **WHEN** a user views the Docker Hub repository
- **THEN** the image name is `janitorr/mittens`

### Requirement: C# namespace convention uses "Mittens"
The root namespace SHALL be `Mittens` instead of `AotMemoryServer`. All sub-namespaces SHALL follow the pattern `Mittens.<area>` (e.g., `Mittens.Models`, `Mittens.Data`, `Mittens.Endpoints`, `Mittens.Application`).

#### Scenario: Source files use new namespace
- **WHEN** a new C# file is created
- **THEN** its namespace begins with `Mittens`

#### Scenario: Assembly name matches namespace
- **WHEN** the project is built
- **THEN** the assembly name is `Mittens`

### Requirement: Docker image and container names use "mittens"
The Docker image SHALL be published as `janitorr/mittens`. Docker Compose service names, container names, and volume names SHALL use the `mittens` prefix.

#### Scenario: Docker Compose starts with new service names
- **WHEN** a user runs `docker compose up -d`
- **THEN** the service is named `mittens` and the container is named `mittens`

#### Scenario: Docker image is pullable under new name
- **WHEN** a user runs `docker pull janitorr/mittens:latest`
- **THEN** the image pulls successfully

### Requirement: CLI binary is named "Mittens"
The compiled AOT binary SHALL be named `Mittens` (or `Mittens.exe` on Windows). The `dotnet run` project path SHALL reference `src/Mittens`.

#### Scenario: Running the server with dotnet
- **WHEN** a user runs `dotnet run --project src/Mittens`
- **THEN** the server starts successfully

#### Scenario: AOT binary executes
- **WHEN** a user runs `./Mittens`
- **THEN** the server starts successfully

### Requirement: MCP tool names use "mittens" convention
MCP tool names SHALL use a `mittens_` prefix instead of `memory_` prefix. Tool method names SHALL use `Mittens` prefix instead of `Memory` prefix.

#### Scenario: MCP tools are listed with new prefix
- **WHEN** an agent calls the MCP tools/list endpoint
- **THEN** tool names begin with `mittens_` (e.g., `mittens_list`, `mittens_set`)

### Requirement: Database table name uses "Mittens" convention
The SQLite table containing memory facts SHALL be renamed from `MemoryFacts` to `MittensFacts`. The schema name (if used) SHALL be `mittens`.

#### Scenario: Table is created with new name
- **WHEN** the database is initialized
- **THEN** the table `MittensFacts` is created

### Requirement: All documentation references use new name
All documentation files (README.md, SETUP.md, AGENTS.md, AGENTS.template.md) SHALL reference "Mittens" and "mittens" consistently. No references to "AOT Memory Server" or "aot-memory-server" SHALL remain.

#### Scenario: SETUP guide uses new names
- **WHEN** an LLM reads SETUP.md
- **THEN** all commands reference `mittens` and `janitorr/mittens`

### Requirement: GitHub repository is renamed to "mittens"
The GitHub repository SHALL be renamed from `aot-memory-server` to `mittens`. All CI workflow references SHALL use the new repository name.

#### Scenario: CI workflow uses new repository
- **WHEN** a tag is pushed
- **THEN** the CI workflow builds and publishes to `janitorr/mittens` on Docker Hub
