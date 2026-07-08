# memory-fact-store Specification

## Purpose
TBD - created by archiving change document-baseline-specs. Update Purpose after archive.
## Requirements
### Requirement: Memory fact data model
The system SHALL store memory facts with the following fields: Id (auto-increment integer), Category (string), Key (string), Value (string), Scope (string), Confidence (double, default 1.0), Source (optional string), UpdatedAt (DateTimeOffset), IsDeprecated (boolean).

#### Scenario: Fact is created with all fields
- **WHEN** a new fact is inserted with all fields provided
- **THEN** the fact is stored with all fields intact and Id auto-assigned

### Requirement: Required fields
The system SHALL require Key and Value to be non-empty strings.

#### Scenario: Key is empty
- **WHEN** a fact is validated with an empty or whitespace Key
- **THEN** validation returns an error for Key property

#### Scenario: Value is empty
- **WHEN** a fact is validated with an empty or whitespace Value
- **THEN** validation returns an error for Value property

### Requirement: Value length limit
The system SHALL enforce a maximum Value length of 10,000 characters.

#### Scenario: Value exceeds limit
- **WHEN** a fact is validated with Value length > 10,000
- **THEN** validation returns an error for Value property

### Requirement: Secret detection
The system SHALL reject facts whose Value appears to contain secrets, API keys, or credentials.

#### Scenario: Value contains secret pattern
- **WHEN** a fact is validated and Value matches the secret regex (sk-*, api_key, secret, token, password, private key, -----BEGIN * KEY-----)
- **THEN** validation returns an error for Value property

### Requirement: Category validation
The system SHALL validate Category against the known set: preference, fact, concept, rule, plan, goal, task, note. Unknown categories SHALL produce a warning (not an error).

#### Scenario: Unknown category
- **WHEN** a fact is validated with Category not in the known set
- **THEN** validation returns a warning for Category property

#### Scenario: Known category
- **WHEN** a fact is validated with Category in the known set
- **THEN** validation passes with no Category warning

### Requirement: Unique constraint
The system SHALL enforce uniqueness on the combination of Category, Key, and Scope.

#### Scenario: Duplicate category/key/scope upsert
- **WHEN** upserting a fact with Category, Key, Scope matching an existing fact
- **THEN** conflict resolution applies (see Requirement: Conflict resolution)

### Requirement: Conflict resolution
The system SHALL resolve upsert conflicts by comparing Confidence: the incoming fact replaces the existing one only if Force=true or incoming Confidence > existing Confidence.

#### Scenario: Incoming fact has higher confidence
- **WHEN** upserting a fact with same Category/Key/Scope but higher Confidence
- **THEN** existing fact is replaced with incoming fact

#### Scenario: Force flag overrides confidence
- **WHEN** upserting with Force=true and lower Confidence
- **THEN** incoming fact replaces existing fact

#### Scenario: Existing fact has higher confidence, no force
- **WHEN** upserting with same Category/Key/Scope, lower Confidence, Force=false
- **THEN** existing fact is returned unchanged

### Requirement: UpdateAt timestamp
The system SHALL set UpdatedAt to current UTC time on every create and update operation.

#### Scenario: UpdatedAt set on create
- **WHEN** a new fact is created
- **THEN** UpdatedAt is set to current UTC time

#### Scenario: UpdatedAt updated on modify
- **WHEN** an existing fact is updated
- **THEN** UpdatedAt is set to current UTC time

