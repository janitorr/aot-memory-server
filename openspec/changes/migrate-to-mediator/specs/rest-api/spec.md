## ADDED Requirements

### Requirement: Endpoint dispatch via mediator
The system SHALL dispatch REST requests to their corresponding command/query handlers through a single `IMediator` (or `ISender`) instance rather than resolving individual handler interfaces from the DI container.

#### Scenario: List endpoint sends query through mediator
- **WHEN** GET /api/memory is called
- **THEN** the endpoint sends a GetFacts request via IMediator and returns the PagedResult

#### Scenario: Create endpoint sends command through mediator
- **WHEN** POST /api/memory is called with a valid fact
- **THEN** the endpoint sends an UpsertFact command via IMediator and returns the result

#### Scenario: Handler resolution is centralized
- **WHEN** any /api/memory endpoint executes
- **THEN** the handler is resolved by the mediator from the request scope, not via explicit GetRequiredService<IHandler>
