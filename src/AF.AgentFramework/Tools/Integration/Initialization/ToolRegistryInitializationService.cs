using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AgentFramework.Tools.Registry;
using AgentFramework.Tools.Runtime;

namespace AgentFramework.Tools.Integration.Initialization;

/// <summary>
/// Hosted service that publishes all registered local tools into the registry at startup.
/// Runs once and completes immediately.
/// </summary>
internal sealed class ToolRegistryInitializationService : BackgroundService
{
    private readonly IToolRegistry _registry;
    private readonly IToolRegistryInitializer _initializer;
    private readonly IEnumerable<ILocalTool> _tools;
    private readonly ILogger<ToolRegistryInitializationService> _logger;

    public ToolRegistryInitializationService(
        IToolRegistry registry,
        IToolRegistryInitializer initializer,
        IEnumerable<ILocalTool> tools,
        ILogger<ToolRegistryInitializationService> logger)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
        _tools = tools ?? throw new ArgumentNullException(nameof(tools));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _initializer.Initialize(_registry, _tools);
        var count = _tools.Count();
        _logger.LogInformation("[Tools] Published {count} local tools into registry.", count);
        return Task.CompletedTask;
    }
}
