using AgentFramework.Kernel.Abstractions;
using AgentFramework.Kernel.Profiles;

namespace AgentFramework.Kernel;

public sealed class AgentKernel : IAgentKernel
{
    private readonly ILoopProfile _profile;
    private readonly IReadOnlyList<IKernelMiddleware> _middlewares;

    public KernelStatus Status { get; private set; } = KernelStatus.Stopped;

    public AgentKernel(
        ILoopProfile profile,
        IEnumerable<IKernelMiddleware>? middlewares = null)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        _middlewares = (middlewares ?? Array.Empty<IKernelMiddleware>()).ToArray();
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        if (Status is KernelStatus.Running or KernelStatus.Starting) return Task.CompletedTask;
        Status = KernelStatus.Starting;
        Status = KernelStatus.Running;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        if (Status is KernelStatus.Stopped or KernelStatus.Stopping) return Task.CompletedTask;
        Status = KernelStatus.Stopping;
        Status = KernelStatus.Stopped;
        return Task.CompletedTask;
    }

    public async Task<AgentTickResult> TickAsync(CancellationToken ct = default)
    {
        var started = DateTimeOffset.UtcNow;

        if (Status != KernelStatus.Running)
            return new AgentTickResult(false, started, DateTimeOffset.UtcNow,
                new InvalidOperationException("Kernel must be Running to Tick."));

        var ctx = new KernelContext { CancellationToken = ct };

        try
        {
            foreach (var step in _profile.GetSteps())
            {
                ctx.StepId = step.Id;
                await RunStepAsync(ctx, step.Terminal).ConfigureAwait(false);
            }

            return new AgentTickResult(true, started, DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            Status = KernelStatus.Faulted;
            return new AgentTickResult(false, started, DateTimeOffset.UtcNow, ex);
        }
    }

    private Task RunStepAsync(KernelContext ctx, KernelNext terminal)
    {
        KernelNext next = terminal;

        for (int i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var inner = next;
            next = c => middleware.InvokeAsync(c, inner);
        }

        return next(ctx);
    }
}
