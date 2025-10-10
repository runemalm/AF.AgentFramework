using AgentFramework.Kernel;

namespace AgentFramework.Agents;

/// <summary>
/// Base class for MAPE-K agents: executes a full MAPE loop each tick.
/// </summary>
public abstract class MapekAgentBase : AgentBase
{
    protected abstract Task MonitorAsync(IAgentContext ctx);
    protected abstract Task AnalyzeAsync(IAgentContext ctx);
    protected abstract Task PlanAsync(IAgentContext ctx);
    protected abstract Task ExecuteAsync(IAgentContext ctx);

    protected override async Task TickAsync(WorkItem tick, IAgentContext ctx)
    {
        ctx.Trace($"Starting MAPE-K loop for {Id}");

        await MonitorAsync(ctx).ConfigureAwait(false);
        await AnalyzeAsync(ctx).ConfigureAwait(false);
        await PlanAsync(ctx).ConfigureAwait(false);
        await ExecuteAsync(ctx).ConfigureAwait(false);

        ctx.Trace($"Completed MAPE-K loop for {Id}");
    }
}
