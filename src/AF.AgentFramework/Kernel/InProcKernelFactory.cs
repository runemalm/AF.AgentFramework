namespace AgentFramework.Kernel;

/// <summary>
/// Simple factory that creates in-process kernel instances.
/// </summary>
public sealed class InProcKernelFactory : IKernelFactory
{
    public IKernel Create(KernelOptions options)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));
        return new InProcKernel(options);
    }
}
