namespace AgentFramework.Tools.Pipeline.Policies;

/// <summary>
/// Placeholder for a circuit breaker policy.
/// Not enforced in the initial slice.
/// </summary>
public sealed class CircuitBreakerPolicy
{
    public int FailureThreshold { get; init; } = 5;
    public int ResetAfterSeconds { get; init; } = 60;

    public string State { get; private set; } = "closed";

    public void RecordFailure()
    {
        // no-op for now
    }

    public void RecordSuccess()
    {
        // no-op for now
    }
}