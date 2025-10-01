# Tool Registry & Binding (Overview)

**Purpose:** provide a single catalog that agents can query to find capabilities, regardless of origin.

## Responsibilities
- Publish tools from **providers** (Local, MCP) into a unified catalog.
- Resolve by **name + version range** with precedence rules and allow/deny lists.
- Expose a **per-agent Toolset** view (bindings, role scoping, policy overlays).
- Emit discovery events for observability.

## Key ideas
- **Namespacing:** local::<pack>::<tool>, mcp::<server>::<tool>
- **Versioning:** prefer semantic ranges; for MCP, pin server version or content hash.
- **Governance:** default-deny for newly discovered MCP tools; explicit allowlist.
- **Precedence:** choose local vs MCP when both exist (policy-driven).

See also: [pipeline.md](pipeline.md), [providers/mcp.md](providers/mcp.md), [contracts/tool-contract.md](../contracts/tool-contract.md)
