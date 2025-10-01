namespace AgentFramework.Tools.Pipeline.Policies;

/// <summary>
/// Placeholder for a budget/cost-governance policy.
/// Not enforced in the initial slice.
/// </summary>
public sealed class BudgetPolicy
{
    public string Id { get; init; } = "default";
    public int MaxUnits { get; init; } = 100;

    public bool TryConsume(int units) => true; // no-op for now
}