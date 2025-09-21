namespace AgentFramework.Kernel.Policies.Defaults;

public sealed class DefaultTimeoutPolicy : ITimeoutPolicy
{
    private readonly TimeoutOptions _options;

    public DefaultTimeoutPolicy(TimeoutOptions? options = null)
        => _options = options ?? new TimeoutOptions(null);

    public TimeSpan? GetTimeout(WorkItem item) => _options.GlobalTimeout;
}
