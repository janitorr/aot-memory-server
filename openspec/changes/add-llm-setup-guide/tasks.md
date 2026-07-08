## 1. Create SETUP.md

- [x] 1.1 Create `SETUP.md` at project root with overview, prerequisites check section, and step-by-step structure
- [x] 1.2 Add compose file section with inlined `docker-compose.memory.yml` YAML content
- [x] 1.3 Add server startup and health verification section with curl health check and port conflict troubleshooting
- [x] 1.4 Add MCP configuration section for OpenCode (`opencode.json`) with merge logic guidance
- [x] 1.5 Add MCP configuration section for Claude Desktop (`claude_desktop_config.json`) with transport settings
- [x] 1.6 Add agent instructions section with instructions to fetch `AGENTS.template.md` from GitHub raw URL
- [x] 1.7 Add end-to-end verification section with `memory_set` + `memory_list` test steps
- [x] 1.8 Add troubleshooting section covering port conflicts, Docker not running, stale containers, missing curl, and network issues
- [x] 1.9 Add management commands section (start, stop, logs, reset)

## 2. Create AGENTS.template.md

- [x] 2.1 Create `AGENTS.template.md` at project root with available tools table (memory_list, memory_get, memory_search, memory_set, memory_update, memory_delete)
- [x] 2.2 Add "When to Use" section with memory usage heuristics
- [x] 2.3 Add Categories section with enum values (preference, fact, concept, rule, plan, goal, task, note)
- [x] 2.4 Add Scope Convention section with examples
- [x] 2.5 Add Data Model section with fact JSON schema and field descriptions
- [x] 2.6 Verify AGENTS.template.md does NOT contain Docker startup, release, or CI/CD instructions

## 3. Update README.md

- [x] 3.1 Update "Agent setup instructions" section to point LLMs to `SETUP.md`
- [x] 3.2 Verify "Using with opencode" section for humans remains intact and functional
- [x] 3.3 Add link to `SETUP.md` in the Docker section for users who want LLM-assisted setup

## 4. Verification

- [x] 4.1 Verify SETUP.md contains all required sections per spec (prerequisites, compose, startup, MCP, agent instructions, verification, troubleshooting, management)
- [x] 4.2 Verify AGENTS.template.md contains only tool-usage content (no repo-specific instructions)
- [x] 4.3 Verify README.md changes don't break existing human-facing setup instructions
- [x] 4.4 Run `openspec validate add-llm-setup-guide` to confirm all artifacts pass validation
