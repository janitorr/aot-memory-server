## ADDED Requirements

### Requirement: List facts with filtering and pagination
The system SHALL provide a GET /api/memory endpoint that returns paginated facts filtered by optional category, scope, and key.

#### Scenario: List without filters
- **WHEN** GET /api/memory is called with no query parameters
- **THEN** response is 200 with PagedResult containing all facts (default page 1, pageSize 20)

#### Scenario: Filter by category
- **WHEN** GET /api/memory?category=fact is called
- **THEN** response contains only facts with Category="fact"

#### Scenario: Filter by scope
- **WHEN** GET /api/memory?scope=project is called
- **THEN** response contains only facts with Scope="project"

#### Scenario: Filter by key
- **WHEN** GET /api/memory?key=mykey is called
- **THEN** response contains only facts with Key="mykey"

#### Scenario: Pagination page parameter
- **WHEN** GET /api/memory?page=2&pageSize=10 is called
- **THEN** response contains page 2 with 10 items per page

#### Scenario: Page clamped to minimum 1
- **WHEN** GET /api/memory?page=0 is called
- **THEN** page is treated as 1

#### Scenario: PageSize clamped to 1-100
- **WHEN** GET /api/memory?pageSize=200 is called
- **THEN** pageSize is treated as 100

#### Scenario: PageSize minimum 1
- **WHEN** GET /api/memory?pageSize=0 is called
- **THEN** pageSize is treated as 1

### Requirement: Get fact by ID
The system SHALL provide a GET /api/memory/{id} endpoint returning a single fact.

#### Scenario: Fact exists
- **WHEN** GET /api/memory/5 is called and fact with Id=5 exists
- **THEN** response is 200 with the MemoryFact

#### Scenario: Fact not found
- **WHEN** GET /api/memory/999 is called and no fact with Id=999 exists
- **THEN** response is 404

### Requirement: Create fact
The system SHALL provide a POST /api/memory endpoint that creates a new fact or upserts on conflict.

#### Scenario: Valid fact created
- **WHEN** POST /api/memory with valid MemoryFact body
- **THEN** response is 200 with the created/upserted fact (Id assigned)

#### Scenario: Validation error returns 400
- **WHEN** POST /api/memory with invalid fact (empty Key, secret in Value, etc.)
- **THEN** response is 400 with ErrorResponse containing validation errors

#### Scenario: Force flag on upsert
- **WHEN** POST /api/memory?force=true with conflicting Category/Key/Scope but lower Confidence
- **THEN** incoming fact replaces existing fact

### Requirement: Update fact
The system SHALL provide a PUT /api/memory/{id} endpoint that updates an existing fact.

#### Scenario: Valid update
- **WHEN** PUT /api/memory/5 with valid MemoryFact body
- **THEN** response is 200 with updated fact

#### Scenario: Fact not found
- **WHEN** PUT /api/memory/999 with valid body
- **THEN** response is 404

#### Scenario: Validation error returns 400
- **WHEN** PUT /api/memory/5 with invalid body
- **THEN** response is 400 with ErrorResponse

### Requirement: Delete fact
The system SHALL provide a DELETE /api/memory/{id} endpoint.

#### Scenario: Fact deleted
- **WHEN** DELETE /api/memory/5 and fact exists
- **THEN** response is 204 No Content

#### Scenario: Fact not found
- **WHEN** DELETE /api/memory/999
- **THEN** response is 404

### Requirement: Health endpoint
The system SHALL provide GET /api/health returning database connectivity status.

#### Scenario: Database healthy
- **WHEN** GET /api/health and database is reachable
- **THEN** response is 200 with {"status":"healthy"}

#### Scenario: Database unreachable
- **WHEN** GET /api/health and database connection fails
- **THEN** response is 503