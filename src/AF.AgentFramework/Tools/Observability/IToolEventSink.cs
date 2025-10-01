using AgentFramework.Tools.Contracts;

namespace AgentFramework.Tools.Observability;

/// <summary>
/// Receives lifecycle events for tool invocations. Implementations may forward to logs,
/// metrics, or tracing systems. The default implementation is a no-op.
/// </summary>
public interface IToolEventSink
{
    void OnInvoked(string correlationId, ToolInvocation invocation, ToolDescriptor? descriptor, DateTimeOffset timestampUtc);
    void OnSucceeded(ToolResult result, DateTimeOffset timestampUtc);
    void OnFailed(ToolResult result, DateTimeOffset timestampUtc);
}

/// <summary>
/// No-op sink used when no observability backend is configured.
/// </summary>
public sealed class NullToolEventSink : IToolEventSink
{
    public void OnInvoked(string correlationId, ToolInvocation invocation, ToolDescriptor? descriptor, DateTimeOffset timestampUtc) { }
    public void OnSucceeded(ToolResult result, DateTimeOffset timestampUtc) { }
    public void OnFailed(ToolResult result, DateTimeOffset timestampUtc) { }
}