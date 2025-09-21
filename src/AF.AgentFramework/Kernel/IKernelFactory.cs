namespace AgentFramework.Kernel;

/// <summary>Factory abstraction so Hosting can create a Kernel without knowing its implementation.</summary>
public interface IKernelFactory
{
    IKernel Create(KernelOptions options);
}
