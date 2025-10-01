# Invocation & Result (Shapes)

Defines the stable wire-level semantics for calling a Tool and receiving its outcome. This contract is origin-agnostic (Local or MCP).

## Invocation

**Fields**
- `toolName` — fully qualified name (e.g., `local::fs.write`, `mcp::github::issues.create`).
- `versionRange?` — preferred version/range (SemVer or provider pin); registry resolves the concrete version.
- `input` — JSON-like payload validated against the tool’s `inputSchema`.
- `correlationId` — stable id for tracing across systems.
- `causationId?` — id of the parent event or invocation that caused this call.
- `idempotencyKey?` — required for `IdempotentWrite`; enables safe retries.
- `headers?` — non-sensitive context (e.g., locale, tenant); **no secrets** here.
- `deadline?` — absolute timestamp; must be ≤ policy timeout.
- `metadata?` — caller-supplied opaque key/values for audit (policy may redact/limit).

**Semantics**
- Validation happens before execution; on failure, return `ContractError`.
- Policies (timeouts, rate limits, circuit, budgets) apply **after** authorization succeeds.
- Retries are permitted only if the tool is `IdempotentWrite` and `idempotencyKey` is present.

## Result

**Envelope**
- `status` — `Ok | Error | Retryable`.
- `output?` — validated/redacted object on success.
- `error?` — structured error (see taxonomy).
- `durationMs` — end-to-end time including policies.
- `attempts` — total tries (≥1).
- `resolvedVersion` — concrete tool version executed.
- `policySnapshot` — effective policy values (timeout, retry, rate-limit ids, circuit state) at execution time.
- `correlationId` — echoed from invocation.
- `origin` — `local` or `mcp::<server>`.

**Status rules**
- `Ok` ⇒ `output` present, `error` absent.
- `Error` ⇒ non-retryable failure; caller should not retry.
- `Retryable` ⇒ failure that **may** succeed on retry (respect backoff hints and policy).

## Error taxonomy

- `ContractError`
  - Input/Output schema violations, unsupported version, missing required fields.
- `PolicyError`
  - `Timeout`, `RateLimited`, `CircuitOpen`, `BudgetExceeded`, `ConcurrencyLimited`.
- `AuthError`
  - Missing/invalid credentials, forbidden scope/binding.
- `ExecutionError`
  - Downstream/service/network failure; map provider codes where possible.
- `SystemError`
  - Unexpected runtime faults, serialization errors not covered above.

**Error fields**
- `code` — stable symbolic code (e.g., `Timeout`, `RateLimited`, `SchemaInvalid`).
- `message` — human-readable and sanitized.
- `details?` — machine-readable info (e.g., `retryAfterMs`, `missingField`).
- `isRetryable` — boolean aligned with policy and effect level.
- `causedBy?` — nested/inner cause with limited depth.
- `origin` — `local` or `mcp::<server>`.

## Correlation & causation

- Always propagate `correlationId`; generate one if missing.
- Set `causationId` when an invocation is triggered by a prior event or tool.
- Tracing spans should link on these ids for end-to-end visibility.

## Streaming (optional)

Some providers stream partial results. When streaming is enabled by a caller:
- The `Result` envelope is delivered at completion; partials are emitted as events/frames out-of-band.
- Partial frames carry `correlationId` and sequence numbers.
- The final envelope includes `status` and any terminal `error`.

## Caching

- Allowed only for `Pure` tools.
- Cache key should include `toolName`, `resolvedVersion`, and a canonicalized `input`.
- Cache metadata must be omitted/redacted in logs unless explicitly allowed.

## Pagination & long-running patterns

- Prefer explicit `pageToken`/`nextToken` fields in `input`/`output` for list operations.
- For long-running operations, return `Ok` with an operation `id` and expose status via a separate tool; avoid indefinite holds.

## Backoff hints

When returning `Retryable` or `PolicyError.RateLimited`, include:
- `retryAfterMs?` — recommended client backoff.
- `throttlingScope?` — which limiter (tool id or provider id) applied.
- `circuitState?` — `open | half-open | closed` if a circuit breaker triggered.

## Redaction

- Apply redaction before emitting events, traces, or ledger writes.
- Include a `redactions` array in `policySnapshot` or envelope metadata if fields were removed.

## See also

- [tool-contract.md](tool-contract.md)
- [../policies.md](../policies.md)
- [../observability.md](../observability.md)
