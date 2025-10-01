# Tool Contract (Canonical Fields)

A Tool is a named, versioned capability with a clear contract. The contract is the source of truth for validation, observability, and policy enforcement.

## Identity

- **name**: string, namespaced. Examples:
  - `local::fs.write`
  - `mcp::github::issues.create`
- **version**: string. Prefer SemVer (`1.2.3`) for local tools. For MCP imports, record the server version and optionally a tool content hash.

## Description

- **title**: short, human-readable name.
- **description**: concise purpose and scope (what it does; what it does *not* do).
- **owner**: team/contact responsible for the tool.
- **tags**: list of classification tags (e.g., `comm`, `code`, `calendar`, `read`, `write`).

## Contract (I/O)

- **inputSchema**: JSON-like schema describing required/optional fields, enums, formats, units, and constraints.
- **outputSchema**: JSON-like schema for successful outputs (monotonic evolution: additive fields are allowed; breaking changes require a new major version).
- **examples**: small set of typical and edge-case inputs/outputs for docs and tests.

## Effects & idempotency

- **effect**: one of:
  - `Pure` — read-only, cacheable.
  - `IdempotentWrite` — writes but safe to retry with an `IdempotencyKey`.
  - `NonIdempotentWrite` — writes with non-repeatable side effects; **no automatic retries**.
  - `ExternalSideEffects` — explicit note of real-world or irreversible effects.
- **idempotencyKeyRequirement**: `required | optional | none` (usually `required` for `IdempotentWrite`).

## Policies (defaults)

Defaults can be overridden by host bindings or environment settings:

- **timeoutMs**: execution deadline for a single attempt.
- **retryPolicy**: for `IdempotentWrite` only (strategy, maxAttempts, backoff, jitter).
- **rateLimit**: tokens per interval (tool-level).
- **concurrency**: max in-flight invocations.
- **circuitBreaker**: thresholds (error rate/latency), cooldowns.
- **budgets**: optional time/cost budgets per agent/role/environment.

## Security

- **requiredScopes**: logical permissions or roles required to invoke.
- **secretRefs**: names of secrets the tool needs (resolved via `ToolContext`), never inline values.
- **redactionRules**: fields or paths to redact in inputs/outputs/events/traces/ledger.
- **dataClassification**: e.g., `public | internal | confidential`.

## Error model

Map failures into a stable taxonomy:

- `ContractError` — input/output validation issues; unsupported versions.
- `PolicyError` — timeout, circuit open, rate-limited, budget exceeded.
- `AuthError` — authN/authZ problems, missing/invalid secrets.
- `ExecutionError` — provider/backend fault; remote service errors.
- `SystemError` — unexpected runtime failure.

Each error includes: `code`, `message` (sanitized), `cause`, `isRetryable`, and correlation fields.

## Observability

- **eventNames**: emit `ToolInvoked`, `ToolSucceeded`, `ToolFailed`, `PolicyApplied`, etc.
- **traceAttributes**: minimally `tool.name`, `tool.version`, `tool.origin`, `effect.level`.
- **metrics**: throughput, latency histograms (`p50`, `p95`, `p99`), error rate, retries, saturation, circuit state.

## Versioning & compatibility

- Prefer SemVer for local tools; breaking changes → new major version.
- For MCP imports, record `serverVersion` and optionally `toolVersion`/hash for pinning.
- Document deprecation timelines; publish telemetry to find callers of older versions.

## Examples (conceptual)

- `local::fs.write`  
  - `effect = IdempotentWrite`, requires `IdempotencyKey`.
  - `inputSchema` includes `path`, `content`, `mode?`.
  - Timeout short; no retries on `NonIdempotentWrite` variants (e.g., `append` without idempotency guarantee).

- `mcp::github::issues.create`  
  - `effect = NonIdempotentWrite` unless the server supports idempotency.
  - Rate limit aligned with GitHub quotas; circuit breaker on sustained failures.
  - Secrets: `GITHUB_TOKEN` via `ToolContext`; redaction rules applied to headers and responses.

## See also

- [invocation-result.md](invocation-result.md)
- [schemas.md](schemas.md)
- [../policies.md](../policies.md)
- [../observability.md](../observability.md)
