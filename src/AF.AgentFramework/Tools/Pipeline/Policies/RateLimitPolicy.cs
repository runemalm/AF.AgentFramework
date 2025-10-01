namespace AgentFramework.Tools.Pipeline.Policies;

/// <summary>
/// Placeholder for a rate-limiting policy.
/// Not enforced in the initial slice.
/// </summary>
public sealed class RateLimitPolicy
{
    public string Id { get; init; } = "default";
    public int PermitsPerSecond { get; init; } = 10;

    public bool TryAcquire() => true; // no-op for now
}