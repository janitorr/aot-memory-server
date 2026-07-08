## ADDED Requirements

### Requirement: LLM setup guide document (SETUP.md)
The repository SHALL contain a `SETUP.md` file at the project root that serves as a self-contained installation playbook for LLMs. The guide SHALL enable an LLM to install and configure the AOT Memory Server for any target project without requiring additional context.

#### Scenario: LLM fetches and reads SETUP.md
- **WHEN** an LLM fetches `SETUP.md` from the repository
- **THEN** it contains all information needed to perform a complete installation, including prerequisites, step-by-step instructions, verification, and troubleshooting

#### Scenario: SETUP.md is written as instructions to the LLM
- **WHEN** reading SETUP.md
- **THEN** the content uses imperative, second-person language directed at the LLM (e.g., "write this file", "run this command"), not at the human user

### Requirement: Prerequisites check
SETUP.md SHALL include commands the LLM can run to verify Docker and Docker Compose are available on the user's system before proceeding with installation.

#### Scenario: Docker availability check
- **WHEN** the LLM runs the prerequisite checks
- **THEN** it can determine whether Docker and Docker Compose are installed and functional

### Requirement: Compose file creation
SETUP.md SHALL instruct the LLM to create a Docker Compose file (`docker-compose.memory.yml`) with the correct service definition, including the Docker image, port mapping, volume for data persistence, and environment variables. The YAML content SHALL be inlined in SETUP.md so the LLM can write it directly.

#### Scenario: LLM writes compose file when none exists
- **WHEN** the target project has no existing `docker-compose.yml`
- **THEN** the LLM writes `docker-compose.memory.yml` with the inlined YAML from SETUP.md

#### Scenario: LLM handles existing compose file
- **WHEN** the target project already has a `docker-compose.yml`
- **THEN** the LLM asks the user whether to merge into the existing file or use a separate `docker-compose.memory.yml`

### Requirement: Server startup and health verification
SETUP.md SHALL instruct the LLM to start the server via Docker Compose and verify it is running by checking the health endpoint at `/api/health`.

#### Scenario: Server starts successfully
- **WHEN** the LLM runs `docker compose -f docker-compose.memory.yml up -d`
- **THEN** the health endpoint at `http://localhost:5070/api/health` returns a 200 response within a reasonable timeout

#### Scenario: Port conflict detected
- **WHEN** port 5070 is already in use and the container fails to start
- **THEN** SETUP.md provides troubleshooting steps including checking logs, identifying the conflicting process, and configuring an alternative port via the `ASPNETCORE_URLS` environment variable

### Requirement: MCP client configuration
SETUP.md SHALL provide MCP configuration instructions for OpenCode (`opencode.json`) and Claude Desktop (`claude_desktop_config.json`), enabling the LLM to configure the correct transport and endpoint for each client.

#### Scenario: OpenCode MCP configuration
- **WHEN** the target project uses OpenCode
- **THEN** the LLM adds or merges the MCP remote configuration into `opencode.json` with `type: "remote"`, `url: "http://localhost:5070/mcp"`, and `enabled: true`

#### Scenario: Claude Desktop MCP configuration
- **WHEN** the target project uses Claude Desktop
- **THEN** the LLM adds the MCP server configuration to `claude_desktop_config.json` with the correct HTTP transport settings pointing to `http://localhost:5070/mcp`

#### Scenario: Existing MCP config is detected
- **WHEN** the target project already has an MCP configuration file
- **THEN** the LLM merges the memory server configuration into the existing file without overwriting other MCP servers

### Requirement: Agent instructions template (AGENTS.template.md)
The repository SHALL contain an `AGENTS.template.md` file at the project root that provides tool-usage-only guidance for target projects. It SHALL NOT contain Docker startup instructions, release instructions, CI/CD details, or any repo-specific maintenance information.

#### Scenario: AGENTS.template.md contains only tool usage
- **WHEN** reading `AGENTS.template.md`
- **THEN** it includes: available tools table, when to use memory, categories enum, scope convention, and data model schema

#### Scenario: AGENTS.template.md excludes repo-specific content
- **WHEN** reading `AGENTS.template.md`
- **THEN** it does NOT include: Docker compose instructions, release/semver instructions, CI/CD pipeline details, or project-specific startup commands

### Requirement: AGENTS.md installation
SETUP.md SHALL instruct the LLM to fetch `AGENTS.template.md` from the repository and save it as `AGENTS.md` in the target project root.

#### Scenario: LLM fetches and installs AGENTS.template.md
- **WHEN** the LLM reaches the agent instructions step
- **THEN** it fetches `AGENTS.template.md` from `https://raw.githubusercontent.com/janitorr/aot-memory-server/main/AGENTS.template.md` and saves it as `AGENTS.md` in the target project root

#### Scenario: Existing AGENTS.md is detected
- **WHEN** the target project already has an `AGENTS.md` file
- **THEN** the LLM asks the user whether to overwrite, merge, or skip

### Requirement: End-to-end verification
SETUP.md SHALL include a verification step that confirms the full setup works by having the LLM test the memory tools after installation.

#### Scenario: LLM verifies memory tools are available
- **WHEN** the LLM completes all setup steps
- **THEN** it confirms that memory tools appear in the available tools list and can successfully call `memory_set` followed by `memory_list` to verify persistence

### Requirement: Troubleshooting section
SETUP.md SHALL include a troubleshooting section covering common failure modes: port conflicts, Docker not running, stale containers, missing curl, and network issues.

#### Scenario: Port conflict troubleshooting
- **WHEN** the LLM encounters a port conflict on 5070
- **THEN** SETUP.md provides commands to identify the conflicting process and instructions to configure an alternative port

#### Scenario: Docker not running troubleshooting
- **WHEN** Docker is not running
- **THEN** SETUP.md provides instructions to start Docker and retry

### Requirement: Management commands
SETUP.md SHALL include commands for managing the memory server after installation: start, stop, view logs, and reset data.

#### Scenario: LLM provides management commands to user
- **WHEN** the user asks how to manage the memory server
- **THEN** the LLM can provide `docker compose -f docker-compose.memory.yml up -d`, `down`, `logs -f`, and `down -v` commands

### Requirement: README.md agent setup pointer
The `README.md` file SHALL be updated to include a pointer directing LLMs to `SETUP.md` for installation guidance, while preserving the existing human-centric "Using with opencode" section.

#### Scenario: README.md points LLMs to SETUP.md
- **WHEN** an LLM reads the README.md "Agent setup instructions" section
- **THEN** it finds a clear pointer to read `SETUP.md` for the complete installation guide

#### Scenario: Human setup docs remain unchanged
- **WHEN** a human reads the README.md "Using with opencode" section
- **THEN** the step-by-step human instructions remain intact and functional
