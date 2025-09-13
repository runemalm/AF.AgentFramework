namespace AgentFramework.Kernel;

public enum KernelStatus
{
    Stopped = 0,
    Starting = 1,
    Running  = 2,
    Stopping = 3,
    Faulted  = 4
}