namespace AgentFramework.Kernel.Policies;

public enum AdmissionDecision { Accept, Defer, Reject }
public enum PreemptionDecision { No, Cooperative }     // Force omitted in v1
public enum BackpressureDecision { Normal, Throttle, Shed }

public sealed record RetryDecision(bool ShouldRetry, TimeSpan? Delay = null, string? Reason = null);
