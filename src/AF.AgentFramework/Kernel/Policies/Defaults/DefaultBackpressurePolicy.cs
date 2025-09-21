namespace AgentFramework.Kernel.Policies.Defaults;

public sealed class DefaultBackpressurePolicy : IBackpressurePolicy
{
    private readonly BackpressureOptions _options;

    public DefaultBackpressurePolicy(BackpressureOptions? options = null)
        => _options = options ?? new BackpressureOptions();

    public BackpressureDecision Evaluate(ClusterLoad load)
    {
        if (load.TotalQueued >= _options.ShedThreshold) return BackpressureDecision.Shed;
        if (load.TotalQueued >= _options.ThrottleThreshold) return BackpressureDecision.Throttle;
        return BackpressureDecision.Normal;
    }
}
