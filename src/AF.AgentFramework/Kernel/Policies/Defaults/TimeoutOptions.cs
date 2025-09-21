namespace AgentFramework.Kernel.Policies.Defaults;

public sealed record TimeoutOptions(TimeSpan? GlobalTimeout = null);
