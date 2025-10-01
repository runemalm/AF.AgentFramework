using AgentFramework.Tools.Contracts;

namespace AgentFramework.Tools.Observability;

/// <summary>
/// Lightweight tracing abstraction for tool invocations.
/// Implementations may bridge to OpenTelemetry or any tracing system.
/// </summary>
public interface IToolTracer
{
    /// <summary>
    /// Starts a tracing scope for the given invocation and descriptor (if resolved).
    /// Returns an IDisposable that must be disposed at the end of the invocation.
    /// </summary>
    IDisposable StartScope(string correlationId, ToolInvocation invocation, ToolDescriptor? descriptor);

    /// <summary>
    /// Attaches result fields (status, error code, duration, etc.) to the current scope.
    /// </summary>
    void AnnotateResult(ToolResult result);
}

/// <summary>
/// No-op tracer used when no tracing backend is configured.
/// </summary>
public sealed class NullToolTracer : IToolTracer
{
    private sealed class Scope : IDisposable { public void Dispose() { } }

    public IDisposable StartScope(string correlationId, ToolInvocation invocation, ToolDescriptor? descriptor) => new Scope();

    public void AnnotateResult(ToolResult result) { }
}