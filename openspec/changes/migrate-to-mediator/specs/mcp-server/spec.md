## ADDED Requirements

### Requirement: MCP tool dispatch via mediator
The system SHALL invoke the underlying command/query handlers for each MCP tool through a single `IMediator` (or `ISender`) instance rather than injecting individual handler interfaces into the tool class.

#### Scenario: MemoryList tool sends query through mediator
- **WHEN** the MemoryList tool is invoked
- **THEN** it sends a GetFacts request via IMediator and returns the serialized PagedResult

#### Scenario: MemorySet tool sends command through mediator
- **WHEN** the MemorySet tool is invoked with valid arguments
- **THEN** it sends an UpsertFact command via IMediator and returns the serialized result

#### Scenario: Tool class has no handler interface dependencies
- **WHEN** the MemoryMcpTools class is constructed
- **THEN** it depends only on IMediator, not on the six individual handler interfaces
