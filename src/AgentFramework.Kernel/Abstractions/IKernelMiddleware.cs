namespace AgentFramework.Kernel.Abstractions;

public interface IKernelMiddleware
{
    Task InvokeAsync(KernelContext context, KernelNext next);
}
