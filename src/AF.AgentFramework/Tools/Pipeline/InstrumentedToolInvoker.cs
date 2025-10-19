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
        // Resolve descriptor for richer event metadata
        ToolDescriptor? descriptor = _registry.Resolve(
            invocation.ToolName,
            invocation.VersionRange,
            new ToolBindingContext { AgentId = invocation.AgentId ?? "unknown" });

        var correlationId = string.IsNullOrWhiteSpace(invocation.CorrelationId)
            ? Guid.NewGuid().ToString("N")
            : invocation.CorrelationId!;

        // Enrich invocation with correlation + agent metadata
        var enrichedInvocation = invocation with
        {
            CorrelationId = correlationId,
            AgentId = invocation.AgentId,
        };

        // Emit "invoked" event
        _events.OnInvoked(correlationId, enrichedInvocation, descriptor, DateTimeOffset.UtcNow);

        using var scope = _tracer.StartScope(correlationId, enrichedInvocation, descriptor);

        // Execute inner pipeline
        var result = await _inner.InvokeAsync(enrichedInvocation, cancellationToken).ConfigureAwait(false);

        // Ensure AgentId and ToolName propagate forward
        var enrichedResult = result with
        {
            AgentId = invocation.AgentId,
            ToolName = invocation.ToolName
        };

        _tracer.AnnotateResult(enrichedResult);

        // Emit success/failure events
        if (enrichedResult.Status == ToolResultStatus.Ok)
            _events.OnSucceeded(enrichedResult, DateTimeOffset.UtcNow);
        else
            _events.OnFailed(enrichedResult, DateTimeOffset.UtcNow);

        return enrichedResult;
    }
}
