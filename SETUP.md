# AOT Memory Server — LLM Setup Guide

You are an LLM assisting a user in setting up the AOT Memory Server for their project. Follow these steps in order.

## Overview

The AOT Memory Server is a lightweight, AOT-compiled persistent memory store for AI agents. It provides REST and MCP APIs backed by SQLite. This guide walks you through a complete installation for any target project.

**What you will set up:**
- A Docker container running the memory server on port 5070
- MCP configuration for the user's AI client (OpenCode or Claude Desktop)
- Agent instructions so future LLM sessions know how to use memory tools

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
  memory-server:
    image: janitorr/aot-memory-server:latest
    container_name: aot-memory-server
    ports:
      - "5070:5070"
    volumes:
      - memory-data:/app/data
    environment:
      - ASPNETCORE_URLS=http://0.0.0.0:5070
      - ConnectionStrings__DefaultDb=Data Source=/app/data/memory.db
    restart: unless-stopped

volumes:
  memory-data:
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

## Step 3: Configure the MCP Client

Configure the user's AI client to connect to the memory server.

### Option A: OpenCode (`opencode.json`)

Add or merge the following into the project's `opencode.json`:

```json
{
  "mcp": {
    "memory": {
      "type": "remote",
      "url": "http://localhost:5070/mcp",
      "enabled": true
    }
  }
}
```

**Merge guidance:** If `opencode.json` already has an `mcp` section, add the `memory` entry alongside any existing MCP servers. Do not overwrite other configurations. If `$schema` is present, preserve it.

### Option B: Claude Desktop (`claude_desktop_config.json`)

Add the following to the user's Claude Desktop configuration file:

```json
{
  "mcpServers": {
    "aot-memory-server": {
      "transport": "http",
      "url": "http://localhost:5070/mcp"
    }
  }
}
```

**Note:** Claude Desktop's MCP HTTP transport support may vary. If HTTP transport does not work, the user may need to use a stdio-based proxy. Refer to the MCP documentation for the latest guidance.

**Merge guidance:** If `claude_desktop_config.json` already has an `mcpServers` section, add the `aot-memory-server` entry without removing existing servers.

## Step 4: Install Agent Instructions

Fetch the agent instructions template and install it in the target project:

```bash
curl -o AGENTS.md https://raw.githubusercontent.com/janitorr/aot-memory-server/main/AGENTS.template.md
```

**If the project already has an `AGENTS.md`:** Ask the user whether to:
1. Overwrite the existing file
2. Merge the content (you will need to read both files and combine them)
3. Skip this step

The `AGENTS.template.md` file contains tool-usage guidance so future LLM sessions know how to use the memory server's tools.

## Step 5: Verify the Setup

Test that everything works end-to-end:

1. **Verify MCP tools are available:** Use your tool discovery mechanism to confirm `memory_set` and `memory_list` appear in the available tools.

2. **Test memory persistence:**
   - Call `memory_set` with a test fact:
     - category: `note`
     - key: `setup-test`
     - value: `Memory server setup verification successful`
     - scope: `project`
   - Call `memory_list` and confirm the test fact appears in the results

3. **Clean up the test fact:** Call `memory_delete` with the ID of the test fact you just created.

If all steps succeed, the setup is complete.

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
1. Confirm the container is running: `docker ps | grep aot-memory-server`
2. Check container logs: `docker compose -f docker-compose.memory.yml logs`
3. Verify the port mapping: `docker port aot-memory-server`
4. Try accessing from inside the container: `docker exec aot-memory-server curl http://localhost:5070/api/health`
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
