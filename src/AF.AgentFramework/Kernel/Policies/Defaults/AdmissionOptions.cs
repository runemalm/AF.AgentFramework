namespace AgentFramework.Kernel.Policies.Defaults;

public sealed record AdmissionOptions(
    int QueueSoftLimit = 32,
    int QueueHardLimit = 256,
    bool RespectDeadline = true
);
