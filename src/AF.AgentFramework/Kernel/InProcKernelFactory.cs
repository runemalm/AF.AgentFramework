using Microsoft.Extensions.Logging;

namespace AgentFramework.Kernel;

/// <summary>
/// Simple factory that creates in-process kernel instances.
/// </summary>
public sealed class InProcKernelFactory : IKernelFactory
{
    private readonly ILogger<InProcKernel>? _logger;

    public InProcKernelFactory(ILogger<InProcKernel>? logger = null)
    {
        _logger = logger;
    }

    public IKernel Create(KernelOptions options)
        => new InProcKernel(options, _logger);
}
