# mcp-server Specification

## Purpose
TBD - created by archiving change document-baseline-specs. Update Purpose after archive.
## Requirements
### Requirement: MCP stateless HTTP transport
The system SHALL expose an MCP server at POST /mcp using stateless HTTP transport (official ModelContextProtocol SDK).

#### Scenario: MCP endpoint accepts POST
- **WHEN** a JSON-RPC request is POSTed to /mcp
- **THEN** the request is processed by the MCP server

### Requirement: MemoryList tool
The system SHALL provide an MCP tool MemoryList that lists facts with optional filtering and pagination.

#### Scenario: List with defaults
- **WHEN** MemoryList is called with no arguments
- **THEN** returns paginated facts (page=1, pageSize=20) as JSON string

#### Scenario: Filter by category
- **WHEN** MemoryList is called with category="fact"
- **THEN** returns only facts with that category

#### Scenario: Filter by scope
- **WHEN** MemoryList is called with scope="project"
- **THEN** returns only facts with that scope

#### Scenario: Filter by key
- **WHEN** MemoryList is called with key="mykey"
- **THEN** returns only facts with that key

#### Scenario: Custom pagination
- **WHEN** MemoryList is called with page=2, pageSize=10
- **THEN** returns page 2 with 10 items per page (clamped 1-100)

### Requirement: MemoryGet tool
The system SHALL provide an MCP tool MemoryGet that retrieves a single fact by ID.

#### Scenario: Fact exists
- **WHEN** MemoryGet is called with existing id
- **THEN** returns the MemoryFact as JSON string

#### Scenario: Fact not found
- **WHEN** MemoryGet is called with non-existent id
- **THEN** returns null as JSON string

### Requirement: MemorySearch tool
The system SHALL provide an MCP tool MemorySearch that searches facts by keyword in Key and Value fields.

#### Scenario: Search with keyword
- **WHEN** MemorySearch is called with q="password"
- **THEN** returns facts where Key LIKE "%password%" OR Value LIKE "%password%"

#### Scenario: Search with filters
- **WHEN** MemorySearch is called with q="config", category="preference"
- **THEN** returns matching facts filtered by category

#### Scenario: Search pagination
- **WHEN** MemorySearch is called with page=1, pageSize=5
- **THEN** returns paginated results (clamped 1-100)

### Requirement: MemorySet tool
The system SHALL provide an MCP tool MemorySet that creates or replaces a fact by Category/Key/Scope.

#### Scenario: Create new fact
- **WHEN** MemorySet is called with category="fact", key="config", value="...", scope="project"
- **THEN** creates new fact and returns it as JSON

#### Scenario: Upsert existing fact
- **WHEN** MemorySet is called with matching Category/Key/Scope
- **THEN** applies conflict resolution (higher Confidence or Force wins)

#### Scenario: Default confidence
- **WHEN** MemorySet is called without confidence parameter
- **THEN** confidence defaults to 1.0

#### Scenario: Validation enforced
- **WHEN** MemorySet is called with invalid Value (secret, too long, etc.)
- **THEN** returns error (validation same as REST)

### Requirement: MemoryUpdate tool
The system SHALL provide an MCP tool MemoryUpdate that partially updates a fact by ID.

#### Scenario: Update existing fact
- **WHEN** MemoryUpdate is called with id and new value
- **THEN** returns updated fact with only changed fields modified

#### Scenario: Fact not found
- **WHEN** MemoryUpdate is called with non-existent id
- **THEN** returns {"error":"Fact not found"} as JSON string

#### Scenario: Partial update
- **WHEN** MemoryUpdate is called with only value provided
- **THEN** only Value is changed; Category, Key, Scope, Confidence, Source remain unchanged

### Requirement: MemoryDelete tool
The system SHALL provide an MCP tool MemoryDelete that removes a fact by ID.

#### Scenario: Delete existing fact
- **WHEN** MemoryDelete is called with existing id
- **THEN** returns "true" as JSON string

#### Scenario: Delete non-existent fact
- **WHEN** MemoryDelete is called with non-existent id
- **THEN** returns "false" as JSON string

### Requirement: All tools return JSON strings
The system SHALL serialize all tool return values as JSON strings using source-generated AOT-safe serializer.

#### Scenario: Tool returns serialized JSON
- **WHEN** any MCP tool returns a value
- **THEN** the value is serialized via AppJsonContext.Default and returned as string

