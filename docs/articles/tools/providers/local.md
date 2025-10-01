# Local Provider (Overview)

**What:** in-process tool implementations that run within the same host as your agents. This gives low latency, simple deployment, and maximal control over secrets and policies.

## When to use local tools
- You need **low latency** or **high throughput**.
- The capability uses **private resources** (filesystem, internal HTTP services).
- You want deterministic behavior for unit/integration tests.
- You have a **local fallback** for an MCP/server outage.

## Namespacing & discovery
- Tools are published as `local::<pack>::<tool>`.
- Packs group related capabilities (e.g., `local::fs.*`, `local::http.*`).
- The registry exposes a unified view; agents don’t care that the origin is local.

## Policy overlays still apply
Local tools run through the same pipeline as MCP tools:
- **Timeouts**, **rate limits**, **concurrency caps**, **circuit breakers**.
- Retries apply **only** if the tool declares `Effect = IdempotentWrite` and an `IdempotencyKey` is provided.

## Secrets & security
- Bind secrets per **agent/role** via `ToolContext`; avoid passing raw tokens through inputs.
- Redact sensitive outputs/fields before events, traces, or ledger writes.
- Prefer least privilege: expose only the filesystem paths, hosts, or operations the agent truly needs.

## Testing guidance
- Provide deterministic stubs/mocks for each local tool.
- Use contract tests derived from the tool’s input/output schemas.
- For replay, couple with the optional ledger to regenerate previous runs.

## Operational notes
- Emit standard events (`ToolInvoked`, `ToolSucceeded`, `ToolFailed`) with `origin = local`.
- Track latency histograms and error rates; set SLOs even for local tools.
- Consider a **composite tool** to route: prefer `local` by default, fall back to `mcp::<server>` when needed.

## See also
- [../index.md](../index.md)
- [../policies.md](../policies.md)
- [../observability.md](../observability.md)
- [../security.md](../security.md)
