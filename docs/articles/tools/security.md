# Security & Secrets (Overview)

Design goals:
- Least-privilege by default (per agent/role scoping).
- No secrets leak into logs, traces, or ledgers.
- Clear, enforceable effect model to control retries/caching.

## Secret management

- **Providers**: pluggable secret sources (env vars, files, cloud vaults).
- **Scope**: secrets are bound to an **agent/role**, not global.
- **Access**: expose via `ToolContext` with explicit key names; avoid passing raw tokens through tool inputs.
- **Rotation**: support hot-rotation (reload without restart); prefer short-lived tokens for MCP servers.

## Redaction

- Redact known sensitive fields before:
  - emitting events/logs,
  - capturing traces,
  - writing to the ledger (if enabled).
- Include a `redactions` array so consumers know what was removed.
- Default-redact common patterns (e.g., `token`, `authorization`, `password`, `secret`, `apikey`).

## Effect model & safety

- `Pure`: never writes externally; cacheable; no retries needed.
- `IdempotentWrite`: safe to retry with an `IdempotencyKey`.
- `NonIdempotentWrite`: **no automatic retries**; prefer explicit compensation tools.
- `ExternalSideEffects`: document consequences and necessary guardrails (e.g., rate limits, budgets).

## Authorization & binding

- Enforce **allowlists** for imported MCP tools; default-deny on discovery.
- Bind tools to agents/roles; deny if the caller lacks the binding.
- For MCP servers, separate:
  - **server access** (who can connect),
  - **tool access** (which tools are callable),
  - **scope** (what resources/permissions the server token grants).

## Policy interplay

- Timeouts: fail fast to avoid credential exposure via long stack traces.
- Retries: only on `IdempotentWrite`; ensure requests are idempotent upstream (e.g., idempotency headers).
- Budgets: cap external spend or time-per-interval; deny with clear errors when exceeded.

## Audit (ledger)

- Store **redacted** invocation/result pairs with correlation IDs.
- Gate access to the ledger; treat it as sensitive.
- Set retention appropriately; consider encryption at rest if storing in external systems.

## Configuration hygiene

- Keep secret names (not values) in source control; values in vaults or CI secrets.
- Separate dev/staging/prod credentials; never reuse tokens across environments.
- Validate that `docfx.json`, `toc.yml`, and docs do not accidentally include secrets in examples.
