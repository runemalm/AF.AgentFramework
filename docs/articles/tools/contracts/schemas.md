# Schemas & Validation (Conventions)

These conventions keep tool inputs/outputs predictable, evolvable, and easy to validate across Local and MCP providers.

## Design principles

- **Contract-first**: define `inputSchema` and `outputSchema` before implementation.
- **Monotonic evolution**: add fields; avoid breaking renames or type changes.
- **Human + machine**: schemas should be readable and enforceable.
- **Explicitness**: prefer enums, formats, and units to free-form text.

## Types & formats

- **Strings**
  - Use formats: `date-time` (RFC 3339, UTC preferred), `uri`, `email`, `uuid`.
  - For identifiers, prefer `id` (opaque string) or `{entity}Id` (e.g., `issueId`).
  - Use `pattern` only when necessary; document examples.

- **Numbers**
  - Include units in the field name or a sibling field, e.g., `timeoutMs`, `amount` + `currency`.
  - For decimals (money), use strings or fixed-precision decimals; avoid float rounding issues.

- **Booleans**
  - Name as predicates: `includeArchived`, `dryRun`, `force`.

- **Arrays**
  - Specify `minItems`, `maxItems?`, and `uniqueItems?` when relevant.
  - For large lists, prefer pagination patterns (see below).

- **Objects**
  - Prefer shallow, composable shapes; avoid deeply nested polymorphism.
  - Mark `required` fields clearly and keep them stable across versions.

## Common fields

- `id` / `{entity}Id`: opaque identifier.
- `createdAt` / `updatedAt`: ISO 8601 `date-time` in UTC.
- `nextToken` / `pageToken`: pagination cursors (opaque strings).
- `locale`: BCP 47 language tag (e.g., `en-US`).
- `tenantId` / `projectId`: scoping identifiers where applicable.

## Pagination

Use a consistent cursor pattern:

```json
{
  "input": {
    "pageSize": 50,
    "pageToken": "opaque-cursor"
  },
  "output": {
    "items": [ /* ... */ ],
    "nextToken": "opaque-cursor-or-null"
  }
}
```

- `pageSize` limits per page; cap server-side to prevent overload.
- `nextToken` is opaque; callers must not inspect or construct it.

## Partial fields & nullability

- Omit optional fields when absent; avoid `null` unless it conveys distinct meaning.
- If `null` is meaningful, document it explicitly (e.g., `completedAt: null` means “not completed”).

## Enums & unions

- Prefer **string enums** for bounded sets:

```json
{
  "status": {
    "type": "string",
    "enum": ["queued", "running", "succeeded", "failed"],
    "description": "Terminal states are 'succeeded' or 'failed'."
  }
}
```

- For unions (one of several shapes), use `oneOf` with discriminators:

```json
{
  "type": "object",
  "oneOf": [
    { "required": ["type"], "properties": { "type": { "const": "file" }, "path": { "type": "string" } } },
    { "required": ["type"], "properties": { "type": { "const": "url" }, "href": { "type": "string", "format": "uri" } } }
  ]
}
```

## Timeouts & deadlines

- Prefer absolute deadlines from the caller (`deadline`) over just durations.
- If durations are needed, encode in milliseconds (`timeoutMs`) as integers.

## Errors & validation

Map schema validation outcomes to the error taxonomy:

- **Missing required** → `ContractError` with `code = "RequiredMissing"`, include `field`.
- **Type mismatch / format** → `ContractError` `code = "InvalidType" | "InvalidFormat"`, include `field`, `expected`, `actual`.
- **Enum violation** → `ContractError` `code = "InvalidEnumValue"`, include `field`, `allowed`.
- **OneOf failure** → `ContractError` `code = "DiscriminatorMismatch"`, include candidate summary.

**Error shape (example):**

```json
{
  "status": "Error",
  "error": {
    "code": "RequiredMissing",
    "message": "Field 'path' is required.",
    "details": { "field": "path" }
  }
}
```

## Output evolution (monotonic)

- You may **add** optional fields at any time.
- Do **not** remove or change types of existing fields in the same major version.
- For breaking changes, bump the tool **major** version (or pin a new MCP tool version) and keep old behavior until callers migrate.

## Redaction rules

- Declare redaction paths for PII/secrets (e.g., `headers.authorization`, `token`, `password`).
- Redaction should apply to:
  - logs and events,
  - traces,
  - ledger entries.
- Emit a `redactions` list with the paths removed.

## Examples (concise)

**Idempotent write with idempotency key:**

```json
{
  "input": {
    "path": "/tmp/data.json",
    "content": "SGVsbG8=",
    "encoding": "base64"
  },
  "effect": "IdempotentWrite",
  "policies": { "timeoutMs": 1500 },
  "requires": { "idempotencyKey": true }
}
```

**Pure read with caching hint:**

```json
{
  "input": {
    "href": "https://api.example.com/status"
  },
  "effect": "Pure",
  "cache": { "ttlMs": 30000 }
}
```

## Streaming (optional)

If a tool supports streaming partials, define a chunk schema:

```json
{
  "chunk": {
    "type": "object",
    "required": ["correlationId", "sequence", "event", "data"],
    "properties": {
      "correlationId": { "type": "string", "format": "uuid" },
      "sequence": { "type": "integer", "minimum": 0 },
      "event": { "type": "string", "enum": ["partial", "heartbeat", "completed", "error"] },
      "data": { "type": "object" }
    }
  }
}
```

- The final non-streaming `Result` envelope still contains `status` and any terminal `error`.

## Schema location & docs

- Co-locate schemas with tool docs for discoverability.
- Include at least one valid and one invalid example for tests.
- Keep a short changelog per tool contract to track evolution.

## See also

- [tool-contract.md](tool-contract.md)
- [invocation-result.md](invocation-result.md)
- [../policies.md](../policies.md)
