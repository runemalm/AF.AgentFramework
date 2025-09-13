using AgentFramework.Kernel;
using AgentFramework.Kernel.Abstractions;
using AgentFramework.Kernel.Profiles;

var profile = new TestProfile();
var middlewares = new IKernelMiddleware[] { new LoggingMiddleware() };

var kernel = new AgentKernel(profile, middlewares);
await kernel.StartAsync();
var result = await kernel.TickAsync();

Console.WriteLine(result.Success ? "Tick OK ✅" : $"Tick FAILED ❌: {result.Error?.Message}");

sealed class LoggingMiddleware : IKernelMiddleware
{
    private readonly Action<string> _log = Console.WriteLine;
    public async Task InvokeAsync(KernelContext context, KernelNext next)
    {
        _log($"[Kernel] → {context.StepId}");
        try { await next(context); _log($"[Kernel] ← {context.StepId} (ok)"); }
        catch (Exception ex) { _log($"[Kernel] ← {context.StepId} (error: {ex.Message})"); throw; }
    }
}

sealed class TestProfile : ILoopProfile
{
    public string Name => "TestProfile";
    public IReadOnlyList<LoopStepDescriptor> GetSteps()
        => new[]
        {
            new LoopStepDescriptor(new LoopStepId("StepA"), StepA),
            new LoopStepDescriptor(new LoopStepId("StepB"), StepB),
            new LoopStepDescriptor(new LoopStepId("End"),   End)
        };

    private static Task StepA(KernelContext ctx) { ctx.Items["a"] = true; return Task.CompletedTask; }
    private static Task StepB(KernelContext ctx) { ctx.Items["b"] = true; return Task.CompletedTask; }
    private static Task End  (KernelContext ctx) { ctx.Items["end"] = true; return Task.CompletedTask; }
}