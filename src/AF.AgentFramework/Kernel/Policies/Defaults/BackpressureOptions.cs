namespace AgentFramework.Kernel.Policies.Defaults;

public sealed record BackpressureOptions(
    int ThrottleThreshold = 10_000,
    int ShedThreshold = 50_000
);
