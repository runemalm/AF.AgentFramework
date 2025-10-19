using System.Collections.Concurrent;
using AgentFramework.Kernel.Diagnostics;
using AgentFramework.Tools.Contracts;

namespace AgentFramework.Tools.Observability;

/// <summary>
/// Aggregates per-agent tool usage metrics and exposes them to the kernel snapshot.
/// Also listens to tool lifecycle events emitted by the <see cref="InstrumentedToolInvoker"/>.
/// </summary>
public sealed class ToolsMetricsProvider : IToolEventSink, IAgentMetricsProvider
{
    private readonly ConcurrentDictionary<string, ToolStats> _byAgent = new(StringComparer.Ordinal);

    public string Name => "Tools";

    // ----------------------------
    // IToolEventSink implementation
    // ----------------------------

    public void OnInvoked(string correlationId, ToolInvocation invocation, ToolDescriptor? descriptor, DateTimeOffset timestampUtc)
    {
        if (invocation.AgentId is null) return;

        var stats = _byAgent.GetOrAdd(invocation.AgentId, _ => new ToolStats());
        stats.RecordInvocation(descriptor?.Name ?? invocation.ToolName);
    }

    public void OnSucceeded(ToolResult result, DateTimeOffset timestampUtc)
    {
        if (result.AgentId is null) return;

        if (_byAgent.TryGetValue(result.AgentId, out var stats))
            stats.RecordSuccess(result.ToolName);
    }

    public void OnFailed(ToolResult result, DateTimeOffset timestampUtc)
    {
        if (result.AgentId is null) return;

        if (_byAgent.TryGetValue(result.AgentId, out var stats))
            stats.RecordFailure(result.ToolName);
    }

    // -------------------------------
    // IAgentMetricsProvider interface
    // -------------------------------

    public AgentMetricsSnapshot? TryGetAgentMetrics(string agentId)
    {
        if (!_byAgent.TryGetValue(agentId, out var stats))
            return null;

        var dict = new Dictionary<string, object>
        {
            ["Tools.TotalInvoked"] = stats.Invoked,
            ["Tools.Errors"] = stats.Errors,
            ["Tools.LastTool"] = stats.LastTool ?? "—"
        };

        return new AgentMetricsSnapshot(Name, dict);
    }

    // -------------------------------
    // Internal stats holder
    // -------------------------------

    private sealed class ToolStats
    {
        private int _invoked;
        private int _errors;
        private string? _lastTool;

        public int Invoked => _invoked;
        public int Errors => _errors;
        public string? LastTool => _lastTool;

        public void RecordInvocation(string toolName)
        {
            Interlocked.Increment(ref _invoked);
            _lastTool = toolName;
        }

        public void RecordSuccess(string toolName)
        {
            _lastTool = toolName;
        }

        public void RecordFailure(string toolName)
        {
            Interlocked.Increment(ref _errors);
            _lastTool = toolName;
        }
    }
}
