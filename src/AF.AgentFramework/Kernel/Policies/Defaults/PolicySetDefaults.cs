namespace AgentFramework.Kernel.Policies.Defaults;

/// <summary>
/// Factory for a sane default PolicySet. Override via parameters as needed.
/// </summary>
public static class PolicySetDefaults
{
    /// <summary>
    /// Create a PolicySet populated with default policies.
    /// Pass options or specific policy instances to override pieces.
    /// </summary>
    public static PolicySet Create(
        AdmissionOptions? admission = null,
        RetryOptions? retry = null,
        BackpressureOptions? backpressure = null,
        TimeoutOptions? timeout = null,
        IOrderingPolicy? ordering = null,
        IPreemptionPolicy? preemption = null,
        ISchedulingPolicy? scheduling = null)
    {
        return new PolicySet
        {
            Admission = new DefaultAdmissionPolicy(admission),
            Retry = new DefaultRetryPolicy(retry),
            Backpressure = new DefaultBackpressurePolicy(backpressure),
            Timeout = new DefaultTimeoutPolicy(timeout),
            Ordering = ordering ?? new DefaultOrderingPolicy(),
            Preemption = preemption ?? new DefaultPreemptionPolicy(),
            Scheduling = scheduling ?? new DefaultSchedulingPolicy()
        };
    }
}
