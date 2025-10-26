using AgentFramework.Agents;
using AgentFramework.Kernel;

namespace HelloKernel.Agents;

sealed class HelloMapeAgent : MapekAgentBase
{
    public override string Id { get; }
    private int _loopCount;

    public HelloMapeAgent(string? id = null)
    {
        Id = string.IsNullOrWhiteSpace(id) ? $"hello-mape" : id!;
    }

    protected override async Task MonitorAsync(IAgentContext ctx)
    {
        _loopCount++;
        ctx.Trace($"[M] Monitoring environment — loop #{_loopCount}");
        ctx.Items["temperature"] = 20 + ctx.Random.NextDouble() * 5;
        await RandomDelayAsync(ctx);
    }

    protected override async Task AnalyzeAsync(IAgentContext ctx)
    {
        var temp = (double)ctx.Items["temperature"];
        var decision = temp > 22.5 ? "cool" : "heat";
        ctx.Items["decision"] = decision;
        ctx.Trace($"[A] Analyzing → temperature={temp:F1}, decision={decision}");
        await RandomDelayAsync(ctx);
    }

    protected override async Task PlanAsync(IAgentContext ctx)
    {
        ctx.Trace($"[P] Planning to {ctx.Items["decision"]}");
        await RandomDelayAsync(ctx);
    }

    protected override async Task ExecuteAsync(IAgentContext ctx)
    {
        ctx.Trace($"[E] Executing plan → {ctx.Items["decision"]}");
        await RandomDelayAsync(ctx);
    }

    private static Task RandomDelayAsync(IAgentContext ctx)
    {
        var ms = 100 + ctx.Random.Next(400); // 100–500 ms
        return Task.Delay(ms, ctx.Cancellation);
    }
}
