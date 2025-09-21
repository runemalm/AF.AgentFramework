using System;

namespace AgentFramework.Kernel.Policies.Defaults;

public sealed record RetryOptions(
    int MaxAttempts = 3,
    TimeSpan BaseDelay = default,
    TimeSpan MaxDelay = default,
    bool UseJitter = true
)
{
    public static RetryOptions Default => new(
        MaxAttempts: 3,
        BaseDelay: TimeSpan.FromMilliseconds(250),
        MaxDelay: TimeSpan.FromSeconds(10),
        UseJitter: true
    );
}
