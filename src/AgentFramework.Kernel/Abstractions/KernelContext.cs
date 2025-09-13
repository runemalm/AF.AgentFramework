namespace AgentFramework.Kernel.Abstractions;

public sealed class KernelContext
{
    public LoopStepId StepId { get; internal set; }
    public CancellationToken CancellationToken { get; internal set; }

    // Cross-cutting scratchpad (trace ids, correlation ids, metrics, etc.)
    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();
}
