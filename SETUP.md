# Mittens — LLM Setup Guide

You are an LLM assisting a user in setting up the Mittens for their project. Follow these steps in order.

## Overview

The Mittens is a lightweight, AOT-compiled persistent memory store for AI agents. It provides REST and MCP APIs backed by SQLite. This guide walks you through a complete installation using global OpenCode configuration, so agent instructions and MCP config are shared across all projects. Only one compose file lands in the project root.

**What you will set up:**
- A `docker-compose.memory.yml` file in the project root
- A Docker container running the memory server on port 5070
- Global MCP configuration in `~/.config/opencode/opencode.json`
- Global agent instructions in `~/.config/opencode/mittens.md`

## Prerequisites

Before proceeding, verify the user's system has the required tools. Run these commands:

```bash
# Check Docker is installed and running
docker --version
docker info

# Check Docker Compose is available
docker compose version
```

If any command fails:
- **Docker not found**: Instruct the user to install Docker from https://docs.docker.com/get-docker/
- **Docker not running**: Instruct the user to start Docker Desktop or the Docker daemon
- **docker compose not found**: Compose is bundled with Docker Desktop; if missing, see https://docs.docker.com/compose/install/

Also verify `curl` is available for health checks:

```bash
curl --version
```

If `curl` is not available, use `wget` or PowerShell's `Invoke-WebRequest` as alternatives.

## Step 1: Create the Docker Compose File

Create a file named `docker-compose.memory.yml` in the project root with the following content:

```yaml
services:
  mittens:
    image: janitorr/mittens:latest
    container_name: mittens
    ports:
      - "5070:5070"
    volumes:
      - mittens-data:/app/data
    environment:
      - ASPNETCORE_URLS=http://0.0.0.0:5070
      - ConnectionStrings__DefaultDb=Data Source=/app/data/memory.db
    restart: unless-stopped

volumes:
  mittens-data:
```

**If the project already has a `docker-compose.yml`:** Ask the user whether to:
1. Merge the memory server service into the existing file, or
2. Keep it as a separate `docker-compose.memory.yml` (recommended)

Do not modify the user's existing compose file without their explicit approval.

## Step 2: Start the Server and Verify Health

Start the server:

```bash
docker compose -f docker-compose.memory.yml up -d
```

Wait a few seconds for the container to start, then verify it is running:

```bash
curl http://localhost:5070/api/health
```

You should receive a `200 OK` response. If you get a connection error or non-200 response:

1. Check the container logs:
   ```bash
   docker compose -f docker-compose.memory.yml logs
   ```
2. Look for port binding errors or startup failures
3. See the Troubleshooting section below for common issues

## Step 3: Configure MCP — Global OpenCode Configuration

Configure the user's AI client to connect to the memory server.

### Option A: OpenCode (global `~/.config/opencode/opencode.json`)

Check for the global OpenCode configuration file at `~/.config/opencode/opencode.json`:

**If the file does not exist:** Create it with the following content:

```json
{
  "$schema": "https://opencode.ai/config.json",
  "mcp": {
    "memory": {
      "type": "remote",
      "url": "http://localhost:5070/mcp",
      "enabled": true
    }
  }
}
```

**If the file exists without an `mcp` section:** Add the `mcp` section with the `memory` entry, preserving all existing config.

**If the file exists with other MCP servers:** Add the `memory` entry alongside existing servers. Do not modify or remove other MCP server configurations.

**If `mcp.memory` is already registered:** Skip this step and inform the user that the memory server is already configured globally.

### Option B: Claude Desktop (`claude_desktop_config.json`)

Add the following to the user's Claude Desktop configuration file:

```json
{
  "mcpServers": {
    "mittens": {
      "transport": "http",
      "url": "http://localhost:5070/mcp"
    }
  }
}
```

**Note:** Claude Desktop's MCP HTTP transport support may vary. If HTTP transport does not work, the user may need to use a stdio-based proxy. Refer to the MCP documentation for the latest guidance. Tool guidance for Claude Desktop is deferred — the MCP registration above provides tool descriptions but not the detailed usage guidance that OpenCode receives via the global `instructions` field.

**Merge guidance:** If `claude_desktop_config.json` already has an `mcpServers` section, add the `mittens` entry without removing existing servers.

## Step 4: Install Agent Instructions (Global)

Fetch the agent instructions template and place it in the global OpenCode configuration directory:

```bash
mkdir -p ~/.config/opencode
curl -o ~/.config/opencode/mittens.md https://raw.githubusercontent.com/janitorr/mittens/main/AGENTS.template.md
```

Then register the instruction file in the global `~/.config/opencode/opencode.json`. Check the existing config:

**If the global config has no `instructions` key:** Add `"instructions": ["mittens.md"]` to the config. OpenCode resolves instruction file paths relative to the config file directory, so `"mittens.md"` resolves to `~/.config/opencode/mittens.md`.

**If the global config has an `instructions` array with other entries:** Append `"mittens.md"` to the array if it is not already present. Preserve all existing entries.

**If `mittens.md` is already in the `instructions` array:** Skip this step and inform the user that the instructions are already registered globally.

The `AGENTS.template.md` file contains tool-usage guidance so future LLM sessions know how to use the memory server's tools. With global instructions, every project benefits from a single file — no per-project copies needed.

## Step 5: Create Global AGENTS.md Stub

OpenCode requires a global `~/.config/opencode/AGENTS.md` file to exist for the `instructions` field to load. This file is a minimal stub that tells the model to defer to project-level `AGENTS.md` files.

Check if the file exists:

```bash
test -f ~/.config/opencode/AGENTS.md && echo "EXISTS" || echo "MISSING"
```

**If the file does not exist:** Create it with the following content:

```markdown
# Global Rules

Project-specific rules should be defined in each project's `AGENTS.md`.
```

**If the file exists:** Skip this step. Do not modify or overwrite it — it may already contain global instructions or the same stub.

## Step 6: Verify the Setup

Test that everything works end-to-end:

1. **Verify MCP tools are available globally:** The memory MCP tools (`mittens_set`, `mittens_list`, etc.) should appear in your available tools regardless of which project directory you are in, since the MCP server is registered in the global OpenCode configuration.

2. **Verify global instructions are loaded:** Confirm that the memory server usage guidance from `~/.config/opencode/mittens.md` is present in your system prompt. You should see documentation about categories, scope conventions, and when to use memory tools.

3. **Test memory persistence:**
   - Call `mittens_set` with a test fact:
     - category: `note`
     - key: `setup-test`
     - value: `Memory server setup verification successful`
     - scope: `project`
   - Call `mittens_list` and confirm the test fact appears in the results

4. **Clean up the test fact:** Call `mittens_delete` with the ID of the test fact you just created.

If all steps succeed, the setup is complete.

## Uninstall

To completely remove the Mittens and all its configuration, follow these steps in order.

### Step 1: Stop and Remove the Docker Container

```bash
docker compose -f docker-compose.memory.yml down -v
```

This stops the container and removes the named volume (`mittens-data`), deleting all stored memory facts. **Confirm with the user before running this command** — data cannot be recovered.

**Optional: Remove the Docker image**

```bash
docker rmi janitorr/mittens:latest
```

This frees up disk space but is not required. The image will be re-pulled if the user reinstalls.

### Step 2: Remove the Instruction File

```bash
rm ~/.config/opencode/mittens.md
```

### Step 3: Clean Up Global OpenCode Configuration

Edit `~/.config/opencode/opencode.json` to remove the memory server entries:

1. Remove the `memory` entry from the `mcp` section. If `mcp` becomes empty after removal, you may remove the entire `mcp` section.
2. Remove `"mittens.md"` from the `instructions` array. If `instructions` becomes empty after removal, you may remove the entire `instructions` section.

**Warning:** Preserve all other MCP servers and instruction entries. Only remove entries related to the memory server. Do not modify or remove other configurations.

**If `mcp.memory` is not present:** Skip the MCP cleanup step.

**If `mittens.md` is not in the `instructions` array:** Skip the instructions cleanup step.

**Note:** `~/.config/opencode/AGENTS.md` is a generic global-rules stub, not mittens-specific. Do **not** remove it — it is shared by all global instructions and enables the `instructions` mechanism for any future use.

### Step 4: Remove the Docker Compose File (Optional)

```bash
rm docker-compose.memory.yml
```

This removes the compose file from the project root. Only do this if the user no longer needs it for reference.

After completing these steps, the memory server is fully uninstalled.

## Troubleshooting

### Port 5070 is already in use

**Symptoms:** Container fails to start with a port binding error, or `curl http://localhost:5070/api/health` connects to a different service.

**Steps to resolve:**

1. Identify what is using port 5070:
   ```bash
   # Linux/macOS
   lsof -i :5070
   # or
   ss -tlnp | grep 5070

   # Windows (PowerShell)
   Get-NetTCPConnection -LocalPort 5070
   ```

2. If it is a previous memory server instance:
   ```bash
   docker compose -f docker-compose.memory.yml down
   docker compose -f docker-compose.memory.yml up -d
   ```

3. If another service is using the port, either stop that service or configure the memory server to use a different port by setting `ASPNETCORE_URLS` in the compose file:
   ```yaml
   environment:
     - ASPNETCORE_URLS=http://0.0.0.0:5071
   ports:
     - "5071:5071"
   ```

### Docker is not running

**Symptoms:** `docker compose up -d` fails with "Cannot connect to the Docker daemon."

**Steps to resolve:**
1. Start Docker Desktop or the Docker daemon
2. Wait for Docker to be ready (the Docker Desktop tray icon should be steady)
3. Retry `docker compose -f docker-compose.memory.yml up -d`

### Stale or zombie containers

**Symptoms:** Container exists in a bad state, won't start properly.

**Steps to resolve:**
```bash
# Remove stopped containers
docker compose -f docker-compose.memory.yml down

# Force remove and recreate
docker compose -f docker-compose.memory.yml up -d --force-recreate
```

### Missing curl

**Symptoms:** `curl` command not found.

**Alternatives:**
```bash
# Using wget
wget -qO- http://localhost:5070/api/health

# Using PowerShell (Windows)
Invoke-WebRequest -Uri http://localhost:5070/api/health | Select-Object -ExpandProperty Content

# Using Python
python -c "import urllib.request; print(urllib.request.urlopen('http://localhost:5070/api/health').read().decode())"
```

### Network or connectivity issues

**Symptoms:** Health check times out, container is running but unreachable.

**Steps to resolve:**
1. Confirm the container is running: `docker ps | grep mittens`
2. Check container logs: `docker compose -f docker-compose.memory.yml logs`
3. Verify the port mapping: `docker port mittens`
4. Try accessing from inside the container: `docker exec mittens curl http://localhost:5070/api/health`
5. Check firewall rules that may block localhost connections

## Management Commands

After setup, use these commands to manage the memory server:

```bash
# Start the server
docker compose -f docker-compose.memory.yml up -d

# Stop the server (data persists)
docker compose -f docker-compose.memory.yml down

# View live logs
docker compose -f docker-compose.memory.yml logs -f

# View recent logs
docker compose -f docker-compose.memory.yml logs --tail 100

# Restart the server
docker compose -f docker-compose.memory.yml restart

# Reset — stop and delete all data
docker compose -f docker-compose.memory.yml down -v
```

**Warning:** `down -v` removes the named volume and deletes all stored memory facts. Confirm with the user before running this command.
