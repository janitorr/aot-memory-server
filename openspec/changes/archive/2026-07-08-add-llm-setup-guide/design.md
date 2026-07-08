## Context

Currently, AGENTS.md serves dual purposes: it tells this repo's LLM about memory tool usage AND contains repo-specific maintenance instructions (releases, Docker setup, CI). There is no dedicated guide that an LLM can follow to install the AOT Memory Server for an arbitrary target project. The README's "Agent setup instructions" section is too terse (4 bullet points) to be actionable for an LLM working with a user's project.

## Goals / Non-Goals

**Goals:**
- Provide a self-contained LLM installation guide (`SETUP.md`) that an agent can follow step-by-step to set up the memory server for any project
- Provide a clean `AGENTS.template.md` containing only tool-usage guidance (no repo-specific instructions) for target projects
- Update README.md to point LLMs to SETUP.md while keeping human-centric setup docs intact
- Support both OpenCode and Claude Desktop MCP clients

**Non-Goals:**
- No changes to the server code, API, or Docker image
- No changes to the existing `AGENTS.md` (this repo's own instructions stay as-is)
- No changes to `docker-compose.example.yml` (backward compatibility)
- No support for non-MCP clients (e.g., direct REST API usage by LLMs)

## Decisions

### Decision 1: SETUP.md as a standalone LLM playbook

**Choice:** Write SETUP.md as instructions TO the LLM (imperative, second-person), not TO the human user.

**Rationale:** The LLM fetches this file and executes it. Writing it as a playbook for the LLM eliminates ambiguity about who does what. The LLM reads "you will write this file" rather than "the user should write this file."

**Alternatives considered:**
- Write it as human-facing docs that the LLM interprets — too ambiguous, LLMs vary in interpretation
- Embed it in the Docker image — LLMs can't easily access container filesystem contents

### Decision 2: AGENTS.template.md as a separate file (not inlined in SETUP.md)

**Choice:** Keep AGENTS.template.md as a separate file in the repo. SETUP.md instructs the LLM to fetch it from GitHub raw URL.

**Rationale:** Single source of truth. When we update AGENTS.template.md, all future LLM installations get the latest version without SETUP.md needing updates.

**Alternatives considered:**
- Inline the template in SETUP.md — simpler (one fetch) but duplicates content and requires two files to update when tool guidance changes

### Decision 3: Compose file naming — `docker-compose.memory.yml`

**Choice:** SETUP.md instructs the LLM to create `docker-compose.memory.yml` (not `docker-compose.example.yml`).

**Rationale:** The `.memory.` suffix makes it clear this file is specifically for the memory server. It avoids conflicts with existing `docker-compose.yml` files. The existing `docker-compose.example.yml` stays for backward compatibility and manual downloads.

### Decision 4: Port conflict detection via `curl` health check

**Choice:** After starting the container, verify with `curl http://localhost:5070/api/health`. If it fails, check Docker logs for port binding errors.

**Rationale:** Cross-platform, simple, and tests the actual endpoint. Pre-flight checks with `ss`/`lsof` are platform-specific and unnecessary — Docker will fail to bind if the port is taken, and the health check confirms it's working.

### Decision 5: Existing `docker-compose.yml` — ask the user

**Choice:** If the target project already has a `docker-compose.yml`, the LLM asks the user whether to merge or use a separate file.

**Rationale:** Merging requires understanding the existing compose file structure. The LLM shouldn't blindly modify a user's Docker setup.

## Risks / Trade-offs

| Risk | Mitigation |
|------|-----------|
| LLM fetches AGENTS.template.md from GitHub but the URL changes or file moves | Use a stable URL (main branch), include fallback instructions in SETUP.md |
| Claude Desktop MCP HTTP transport config differs from OpenCode | Include a note that Claude Desktop may require stdio transport via a proxy; link to MCP docs |
| Target project already has an AGENTS.md with different content | SETUP.md instructs LLM to check for existing file and ask before overwriting |
| Port 5070 is already in use by another service | SETUP.md includes troubleshooting: check with `lsof`/`ss`, suggest alternative port via `ASPNETCORE_URLS` env var |
| SETUP.md becomes outdated as MCP clients evolve | Keep SETUP.md versioned in the repo; update as needed with PRs |
