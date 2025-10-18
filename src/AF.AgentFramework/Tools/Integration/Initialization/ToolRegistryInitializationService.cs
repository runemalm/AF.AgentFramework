using AgentFramework.Hosting.Services;
using AgentFramework.Tools.Registry;
using AgentFramework.Tools.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFramework.Tools.Integration.Initialization;

/// <summary>
/// Host service that publishes all registered local tools into the registry at startup.
/// </summary>
internal sealed class ToolRegistryInitializationService : IAgentHostService
{
    public async Task StartAsync(IAgentHostContext context, CancellationToken ct)
    {
        var registry = context.Services.GetRequiredService<IToolRegistry>();
        var tools = context.Services.GetServices<ILocalTool>();
        var initializer = context.Services.GetRequiredService<IToolRegistryInitializer>();

        initializer.Initialize(registry, tools);
        Console.WriteLine($"[Tools] Published {tools.Count()} local tools into registry.");

        await Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
