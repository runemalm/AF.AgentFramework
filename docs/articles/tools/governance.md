# Governance Checklist (Before Adding a Tool)

Use this checklist to keep capabilities safe, observable, and consistent across teams and environments.

## 1) Identity & origin
- Choose a stable name and namespace: `local::<pack>::<tool>` or `mcp::<server>::<tool>`.
- Define ownership: code owners, on-call rotation, escalation path.
- Tag with capability categories (e.g., `comm/slack.post`, `code/github.issue.create`).

## 2) Contract review
- Inputs/outputs defined with clear schemas (required vs optional, enums, units).
- Example payloads and edge cases documented.
- Effect level declared: `Pure` | `IdempotentWrite` | `NonIdempotentWrite` | `ExternalSideEffects`.
- Error taxonomy mapped: `ContractError`, `PolicyError`, `AuthError`, `ExecutionError`, `SystemError`.

## 3) Policy defaults
- Timeouts: set sane defaults (environment-specific).
- Retries: **only** for `IdempotentWrite`, with backoff/jitter and attempt caps.
- Rate limits & concurrency caps: per tool and per provider (bulkheads).
- Circuit breaker thresholds defined.
- Budgets (time/cost) configured where applicable.

## 4) Security & secrets
- Secret scope: per agent/role, not global.
- Auth strategy (API key, OAuth2/JWT) documented; rotation process defined.
- Redaction rules for inputs/outputs/logs/traces/ledger.
- MCP tools: server access separated from tool access; default-deny until allowlisted.

## 5) Observability
- Events emitted: `ToolInvoked`, `ToolSucceeded`, `ToolFailed`, `PolicyApplied`, `RateLimited`, circuit state changes.
- Metrics: throughput, latency (p50/p95/p99), error rate, retries, saturation, circuit state.
- Tracing: span attributes include `tool.name`, `tool.version`, `tool.origin`, `agent.id`, `correlation.id`.
- Dashboards and alerts created (SLOs/SLA documented).

## 6) Testing & quality gates
- Contract tests: valid/invalid inputs, boundary cases, schema evolution checks.
- Failure-path tests: timeouts, rate limits, circuit open, auth failures.
- Deterministic stubs/mocks for local tools; integration tests for MCP tools where feasible.
- (Optional) Ledger-driven replay for critical workflows.

## 7) Versioning & compatibility
- Versioning policy declared (SemVer or server pin/hash for MCP).
- Deprecation plan for breaking changes; telemetry in place to find callers of old versions.
- Registry precedence defined if local and MCP capabilities overlap.

## 8) Operations & rollout
- Rollout plan: staging → canary → prod; feature flags if needed.
- Access control: bindings/allowlists updated for intended agents/roles.
- Runbook: known failure modes, remediation steps, contacts.

## 9) Documentation
- Short description, examples, and caveats in the tool’s docs.
- Link to provider specifics (Local or MCP) and policy overlays.
- Explicit statement of idempotency guarantees and side effects.

## Final gate
- ✅ All above items checked.
- ✅ Owner approved.
- ✅ CI green (tests, lint, schema validation).
