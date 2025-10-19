namespace AgentFramework.Kernel.Diagnostics;

/// <summary>
/// Provides aggregated, per-agent metrics for inclusion in <see cref="KernelSnapshot"/>s.
/// Implementations are typically backed by telemetry emitted from subsystems
/// such as Tools, Reasoning, or Sensors.
/// </summary>
public interface IAgentMetricsProvider
{
    /// <summary>
    /// Logical name of the provider (e.g. "Tools", "Reasoning", "Sensors").
    /// Used as a prefix for metric keys and identification in logs.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Attempts to retrieve the latest metrics for the specified agent.
    /// Returns <c>null</c> if the provider has no data for this agent.
    /// </summary>
    /// <param name="agentId">Unique agent identifier.</param>
    /// <returns>
    /// A snapshot of metrics or <c>null</c> if the provider has no record of the agent.
    /// </returns>
    AgentMetricsSnapshot? TryGetAgentMetrics(string agentId);
}

/// <summary>
/// Immutable view of a provider’s aggregated metrics for one agent.
/// </summary>
/// <param name="ProviderName">The provider’s logical name (e.g. "Tools").</param>
/// <param name="Metrics">Key-value pairs of metric names and values.</param>
public sealed record AgentMetricsSnapshot(
    string ProviderName,
    IReadOnlyDictionary<string, object> Metrics
);
