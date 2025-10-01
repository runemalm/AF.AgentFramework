# Observability

**Goal:** make every tool invocation explainable and diagnosable across Local and MCP providers.

## Events

Emit structured events at key points:
- `ToolInvoked` — name, origin (`local` or `mcp::<server>`), version, `agentId`, `correlationId`, `startTime`.
- `ToolSucceeded` — `durationMs`, `attempts`, `outputSizeBytes?`, `cacheHit?`.
- `ToolFailed` — `error.code`, `error.cause` (`policy|auth|execution|system`), `isRetryable`, `durationMs`, `attempts`.
- `PolicyApplied` — `timeoutMs`, `retryPolicy`, `rateLimit`, `circuitState`, `concurrencyCaps`.
- `RateLimited` — `limiterId`, `tokensRequested`, `retryAfterMs?`.
- `CircuitOpen` / `CircuitHalfOpen` / `CircuitClosed`.

**Recommendations**
- Use stable event names and include an `eventVersion` field for evolution.
- Redact secrets before emission; include a `redactions` list when applied.

## Metrics

Track per **tool + origin** (label both):
- **Throughput**: `tool_invocations_total`.
- **Latency**: histograms (`p50`, `p95`, `p99`) on end-to-end duration.
- **Errors**: `tool_errors_total` by `error.code`.
- **Retries**: `tool_retries_total`.
- **Saturation**: `rate_limiter_saturation`, `concurrency_in_use`.
- **Circuits**: `circuit_state` gauge (0=closed, 1=half-open, 2=open).

**Buckets**
- Keep separate histograms for execution time vs total time (policies may add delay).

## Tracing

Create one span per invocation:
- Span name: `tool:<name>`.
- Attributes: `tool.name`, `tool.version`, `tool.origin`, `agent.id`, `correlation.id`, `conversation.id?`, `effect.level`.
- Link to a parent span if the invocation is caused by an external request or another tool.
- Record exceptions with `error.code` and sanitized message.

## Correlation

- Every invocation carries a `CorrelationId` (and `CausationId` when applicable).
- Propagate across tool calls so multi-tool workflows are traceable.
- For fan-out patterns, create child spans and aggregate with a parent span.

## Ledger (optional)

If enabled, persist redacted records:
- **Invocation**: tool, version, input (redacted), timestamps, correlation.
- **Result**: status, output (redacted), error, duration, attempts, policies applied.

**Retention & access**
- Configure TTL and access controls; treat the ledger as sensitive and consider encryption at rest.

## Log structure (suggested)

Include at minimum for all log lines:
- `ts`, `level`, `event`, `tool.name`, `tool.version`, `origin`, `agent.id`, `correlation.id`, `duration.ms`, `attempts`.
- For failures: `error.code`, `error.class`, `isRetryable`.
- For policy actions: `timeout.ms`, `retry.count`, `rate.limit.id`, `circuit.state`.

## Dashboards (starter set)

- **Tool health**: error rate & latency by tool/origin; top N failing tools.
- **Policy pressure**: rate-limit saturation; circuit states over time.
- **Retries & budgets**: retry counts; total time spent in tools per agent/role.
- **MCP servers**: per-server latency/error overlays vs local equivalents.

## See also

- [policies.md](policies.md)
- [pipeline.md](pipeline.md)
- [security.md](security.md)
- [../contracts/invocation-result.md](../contracts/invocation-result.md)
