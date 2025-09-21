namespace AgentFramework.Kernel.Policies.Defaults;

public sealed class DefaultAdmissionPolicy : IAdmissionPolicy
{
    private readonly AdmissionOptions _options;

    public DefaultAdmissionPolicy(AdmissionOptions? options = null)
        => _options = options ?? new AdmissionOptions();

    public AdmissionDecision Admit(WorkItem item, AgentRuntimeState state)
    {
        // Minimal placeholder logic; real thresholds to be added later.
        if (state.QueueLength >= _options.QueueHardLimit) return AdmissionDecision.Reject;
        if (_options.RespectDeadline && item.Deadline is { } dl && dl < DateTimeOffset.UtcNow) return AdmissionDecision.Reject;
        if (state.QueueLength >= _options.QueueSoftLimit) return AdmissionDecision.Defer;
        return AdmissionDecision.Accept;
    }
}
