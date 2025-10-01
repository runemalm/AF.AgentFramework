namespace AgentFramework.Tools.Contracts;

/// <summary>
/// Declares the side-effect semantics of a tool. This determines which
/// reliability policies (e.g., retries, caching) are allowed by the pipeline.
/// </summary>
public enum EffectLevel
{
    /// <summary>
    /// Read-only and side-effect free. Safe to cache; must not be retried for write semantics
    /// (no writes expected). Failures can be retried by callers as they are idempotent by nature.
    /// </summary>
    Pure = 0,

    /// <summary>
    /// Performs a write but is safe to retry when an idempotency key is supplied.
    /// Pipelines may enable bounded retries with backoff for this effect level.
    /// </summary>
    IdempotentWrite = 1,

    /// <summary>
    /// Performs a write that is not safe to retry automatically (e.g., creates unique resources
    /// without idempotency guarantees). Pipelines must not auto-retry these operations.
    /// </summary>
    NonIdempotentWrite = 2,

    /// <summary>
    /// Indicates real-world or irreversible side effects (e.g., sending emails, triggering payments).
    /// This is a high-signal classification to enforce stricter guardrails, auditing, and consent.
    /// </summary>
    ExternalSideEffects = 3
}
