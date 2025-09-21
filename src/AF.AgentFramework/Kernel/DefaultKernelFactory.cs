namespace AgentFramework.Kernel;

public sealed class DefaultKernelFactory : IKernelFactory
{
    public IKernel Create(KernelOptions options) => new InProcKernel(options);
}
