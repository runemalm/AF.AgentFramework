using AgentFramework.Agents;
using AgentFramework.Kernel;

sealed class HelloMapeAgent : MapekAgentBase
{
    public override string Id => "hello-mape";

    private int _loopCount;

    protected override Task MonitorAsync(IAgentContext ctx)
    {
        _loopCount++;
        ctx.Trace($"[M] Monitoring environment — loop #{_loopCount}");
        ctx.Items["temperature"] = 20 + ctx.Random.NextDouble() * 5;
        return Task.CompletedTask;
    }

    protected override Task AnalyzeAsync(IAgentContext ctx)
    {
        var temp = (double)ctx.Items["temperature"];
        var decision = temp > 22.5 ? "cool" : "heat";
        ctx.Items["decision"] = decision;
        ctx.Trace($"[A] Analyzing → temperature={temp:F1}, decision={decision}");
        return Task.CompletedTask;
    }

    protected override Task PlanAsync(IAgentContext ctx)
    {
        ctx.Trace($"[P] Planning to {ctx.Items["decision"]}");
        return Task.CompletedTask;
    }

    protected override Task ExecuteAsync(IAgentContext ctx)
    {
        ctx.Trace($"[E] Executing plan → {ctx.Items["decision"]}");
        return Task.CompletedTask;
    }
}
