using AgentFramework.Tools.Contracts;
using AgentFramework.Tools.Observability;
using AgentFramework.Tools.Registry;

namespace AgentFramework.Tools.Pipeline;

/// <summary>
/// Decorator that adds observability (events + tracing) around an inner <see cref="IToolInvoker"/>.
/// Does not alter behavior; strictly emits signals before/after invocation.
/// </summary>
public sealed class InstrumentedToolInvoker : IToolInvoker
{
    private readonly IToolInvoker _inner;
    private readonly IToolEventSink _events;
    private readonly IToolTracer _tracer;
    private readonly IToolRegistry _registry;

    public InstrumentedToolInvoker(
        IToolInvoker inner,
        IToolEventSink events,
        IToolTracer tracer,
        IToolRegistry registry)
    {
        _inner = inner;
        _events = events;
        _tracer = tracer;
        _registry = registry;
    }

    public async Task<ToolResult> InvokeAsync(ToolInvocation invocation, CancellationToken cancellationToken = default)
    {
        // Best-effort resolve for richer events (do not fail if not found)
        ToolDescriptor? descriptor = _registry.Resolve(invocation.ToolName, invocation.VersionRange,
            new ToolBindingContext { AgentId = "unknown" });

        var correlationId = string.IsNullOrWhiteSpace(invocation.CorrelationId)
            ? Guid.NewGuid().ToString("N")
            : invocation.CorrelationId!;

        _events.OnInvoked(correlationId, invocation, descriptor, DateTimeOffset.UtcNow);

        using var scope = _tracer.StartScope(correlationId, invocation, descriptor);

        var result = await _inner.InvokeAsync(
            invocation with { CorrelationId = correlationId },
            cancellationToken).ConfigureAwait(false);

        _tracer.AnnotateResult(result);

        if (result.Status == ToolResultStatus.Ok)
            _events.OnSucceeded(result, DateTimeOffset.UtcNow);
        else
            _events.OnFailed(result, DateTimeOffset.UtcNow);

        return result;
    }
}
