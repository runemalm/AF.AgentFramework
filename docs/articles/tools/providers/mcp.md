# MCP Provider (Overview)

**What:** imports tools exposed by external MCP servers and publishes them into the AgentFramework registry under a unified surface.

## Responsibilities

- **Discovery**: connect to configured MCP servers and enumerate available tools.
- **Publication**: register each tool as `mcp::<server>::<tool>` with contracts, metadata, and effect level.
- **Invocation**: translate AF invocations to MCP `call_tool` requests; handle streaming when supported.
- **Safety & policies**: apply AF-side policies (timeouts, retries for idempotent tools, rate limits, circuit breakers, budgets).
- **Security**: manage per-server auth (API keys, OAuth), scoped per agent/role; never leak secrets in events/logs.

## Namespacing & governance

- **Origin tag**: `origin = mcp::<server>`.
- **Allowlist**: default-deny newly discovered tools until explicitly enabled.
- **Version/pin**: track the MCP server version and, if possible, a tool-specific version or content hash for reproducibility.
- **De-duplication**: if a local and an MCP tool offer the same capability, choose precedence via policy (often prefer `local`).

## Contracts & effects

- Map MCP tool schemas to AF contracts:
  - Inputs/outputs must validate under AF’s schema rules.
  - Set the **effect level** (`Pure`, `IdempotentWrite`, `NonIdempotentWrite`, `ExternalSideEffects`) based on server metadata or explicit config.
- Output validation and redaction run after invocation, before events/ledger.

## Invocation flow (high level)

1. Resolve tool in the registry (name + version range, bindings/allowlist).
2. Validate input against the mapped contract.
3. Apply policies (timeouts, rate limits, circuit, budgets).
4. Call MCP `call_tool` with correlation headers/ids.
5. Validate and redact output; emit events/metrics/traces.

## Error handling

- Convert MCP errors into AF error taxonomy:
  - `ContractError` (schema/contract mismatch),
  - `AuthError` (invalid credentials or scope),
  - `ExecutionError` (remote service failure),
  - `PolicyError` (timeout, rate limit, circuit open),
  - `SystemError` (unexpected).
- Mark `isRetryable` only when consistent with the effect level and server semantics.

## Secrets & auth

- Store credentials per server and **scope access per agent/role**.
- Prefer short-lived tokens where available; support rotation without restart.
- Redact all sensitive fields in logs/events/ledger.

## Observability

- Emit standard events with `origin = mcp::<server>`.
- Track latency/error metrics per server and per tool; compare to local equivalents.
- Tag traces with `tool.origin`, `mcp.server`, and correlation ids for cross-system visibility.

## Operational tips

- Start with a minimal allowlist of high-value tools; expand as you gain SLO data.
- Apply stricter defaults (shorter timeouts, tighter rate limits) to new MCP tools.
- Consider composite tools for **fallback** (prefer local, fall back to MCP on outage).

## See also
- [../index.md](../index.md)
- [../policies.md](../policies.md)
- [../observability.md](../observability.md)
- [../security.md](../security.md)
- [../registry.md](../registry.md)
