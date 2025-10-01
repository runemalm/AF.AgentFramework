using AgentFramework.Tools.Contracts;

namespace AgentFramework.Tools.Pipeline;

/// <summary>
/// Applies reliability and safety policies to a tool invocation.
/// Responsible for enforcing deadlines and producing a snapshot of active policies.
/// </summary>
public interface IPolicyApplier
{
    PolicyApplicationResult Apply(ToolInvocation invocation, ToolDescriptor descriptor, CancellationToken ct);
}

/// <summary>
/// Result of applying policies to an invocation.
/// Includes the effective snapshot and an enforced cancellation token.
/// </summary>
public sealed record class PolicyApplicationResult
{
    public PolicySnapshot Snapshot { get; init; } = new();
    public CancellationToken EffectiveToken { get; init; } = CancellationToken.None;
}