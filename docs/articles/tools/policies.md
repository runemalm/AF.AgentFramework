# Policies (Guardrails)

**Goal:** provide production-grade reliability and predictable behavior without leaking provider quirks (Local or MCP).

## Policy types

### Timeouts
Hard deadlines per tool. Provide sensible defaults at environment or role level; allow per-tool overrides. Prefer absolute deadlines over “infinite waits.”

### Retries (idempotent only)
Enabled **only** for tools that declare `Effect = IdempotentWrite` (and supply an IdempotencyKey in the invocation). Use exponential backoff with jitter; cap attempts and total retry duration. Never retry non-idempotent effects automatically.

### Circuit Breakers
Open the circuit on sustained high error rate/latency. Half-open with limited probes; close on success. Protects downstream systems and triggers fast-fail behavior when a provider is unhealthy.

### Rate Limits
Token-bucket or leaky-bucket per tool **and** per provider (bulkheads). Emit `RateLimited` events with retry-after hints. Coordinate with provider quotas when known.

### Concurrency Caps
Bound in-flight invocations per tool/provider to prevent resource exhaustion (threads, sockets, API concurrency). Prefer fair queues when multiple agents share a tool.

### Budgets
Optional time/cost budgets per agent, role, or environment (e.g., “no more than N seconds of tool time per minute,” “no more than X external API cost per day”). Deny or degrade gracefully when budgets are exhausted.

## Resolution & precedence

1. Start from environment defaults (e.g., Production vs Staging).
2. Apply role/agent overlays.
3. Apply per-tool policy config (most specific wins).
4. At runtime, the invocation may specify stricter limits (e.g., shorter timeout), but not looser than allowed by policy.

## Observability requirements

- Emit `PolicyApplied` with the final resolved settings (timeout, retry count, limiter state).
- Track latency percentiles (p50/p95/p99) and error rate per **tool+origin**.
- Expose circuit and limiter saturation metrics for alerting.

## Failure semantics

- **Timeout** → `PolicyError.Timeout` (include elapsed and configured deadline).
- **Rate limited** → `PolicyError.RateLimited` (include retry-after hint if available).
- **Circuit open** → `PolicyError.CircuitOpen`.
- **Budget exceeded** → `PolicyError.BudgetExceeded`.

## Configuration guidance

- Prefer safe defaults: shortish timeouts, small retry budgets, conservative concurrency.
- Keep overrides explicit and reviewable in source control.
- For MCP imports, apply stricter defaults until SLOs are known.
- Document idempotency guarantees per tool; retries depend on them.

## See also

- [pipeline.md](pipeline.md)
- [observability.md](observability.md)
- [security.md](security.md)
