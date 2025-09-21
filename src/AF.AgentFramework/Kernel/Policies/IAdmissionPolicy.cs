namespace AgentFramework.Kernel.Policies;

public interface IAdmissionPolicy
{
    AdmissionDecision Admit(WorkItem item, AgentRuntimeState state);
}
