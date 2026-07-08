## Why

The current AGENTS.md mixes this repo's maintenance instructions (releases, Docker setup for this project) with general memory tool usage guidance. There is no dedicated, self-contained guide that an LLM can follow to install and configure the AOT Memory Server for an arbitrary target project. When someone pulls the Docker image and asks their LLM to set up memory, the LLM lacks a clear, step-by-step playbook.

## What Changes

- **Add `SETUP.md`** — A comprehensive, self-contained LLM installation guide. Includes prerequisites checks, compose file creation (inline YAML), MCP configuration for OpenCode and Claude Desktop, port conflict detection, verification steps, and troubleshooting.
- **Add `AGENTS.template.md`** — A clean, tool-usage-only version of AGENTS.md for target projects. No Docker startup, no release instructions, no CI details. Just: available tools, when to use, categories, scope convention, and data model.
- **Update `README.md`** — Shrink the "Agent setup instructions" section to a pointer at `SETUP.md`. Keep the human-centric "Using with opencode" section unchanged.
- **Update `docker-compose.example.yml`** — Rename to `docker-compose.memory.yml` naming convention in SETUP.md (the existing file stays for backward compatibility).

## Capabilities

### New Capabilities

- `llm-setup-guide`: Documentation artifacts that enable an LLM to install and configure the AOT Memory Server for any target project, including compose file creation, MCP client configuration, agent instructions, and verification.

### Modified Capabilities

<!-- No existing spec requirements are changing. This is a documentation-only change. -->

## Impact

- **New files**: `SETUP.md`, `AGENTS.template.md`
- **Modified files**: `README.md` (Agent setup instructions section)
- **No code changes**: No server-side code, API changes, or breaking changes
- **No Docker image changes**: The Docker image and compose file are unchanged
