namespace AgentFramework.Tools.Pipeline.Policies;

/// <summary>
/// Placeholder for a concurrency-limiting policy.
/// Not enforced in the initial slice.
/// </summary>
public sealed class ConcurrencyPolicy
{
    public string Id { get; init; } = "default";
    public int MaxConcurrent { get; init; } = 1;

    public bool TryEnter() => true; // no-op for now
    public void Exit() { }
}