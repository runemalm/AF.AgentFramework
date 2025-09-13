using Xunit;
using AgentFramework.Kernel.Abstractions;
using AgentFramework.Kernel.Profiles;

namespace AgentFramework.Kernel.Tests;

public class KernelSmokeTests
{
    [Fact]
    public async Task Tick_runs_all_steps_in_order_and_succeeds()
    {
        var profile = new TestProfile();                       // sample-only, defined below
        var seen = new List<string>();
        var mw = new DelegatingMiddleware(async (ctx, next) =>
        {
            seen.Add("→ " + ctx.StepId);
            await next(ctx);
            seen.Add("← " + ctx.StepId);
        });

        var kernel = new AgentKernel(profile, new IKernelMiddleware[] { mw });
        await kernel.StartAsync();
        var result = await kernel.TickAsync();

        Assert.True(result.Success);
        Assert.Equal(new[] { "→ StepA", "← StepA", "→ StepB", "← StepB", "→ End", "← End" }, seen);
    }
}

/// <summary>Inline test helper that composes before/after around the next delegate.</summary>
file sealed class DelegatingMiddleware : IKernelMiddleware
{
    private readonly Func<KernelContext, KernelNext, Task> _impl;
    public DelegatingMiddleware(Func<KernelContext, KernelNext, Task> impl) => _impl = impl;
    public Task InvokeAsync(KernelContext context, KernelNext next) => _impl(context, next);
}

/// <summary>
/// Tiny test-only profile: three inert steps to exercise ordering and middleware wrapping.
/// </summary>
file sealed class TestProfile : ILoopProfile
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
