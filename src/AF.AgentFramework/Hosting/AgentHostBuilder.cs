using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AgentFramework.Kernel;
using AgentFramework.Kernel.Policies;
using AgentFramework.Engines;
using AgentFramework.Runners;
using AgentFramework.Hosting.Services;
using Microsoft.Extensions.Logging;

namespace AgentFramework.Hosting;

/// <summary>
/// Builder for composing and configuring an AgentHost.
/// Wraps a .NET <see cref="HostApplicationBuilder"/> and registers all agents,
/// engines, runners, tools, and hosted services into the DI container.
/// </summary>
public sealed class AgentHostBuilder : IAgentHostBuilder
{
    private readonly HostApplicationBuilder _appBuilder;

    public static AgentHostBuilder Create(string[]? args = null)
        => new(args ?? Array.Empty<string>());

    private AgentHostBuilder(string[] args)
    {
        _appBuilder = Host.CreateApplicationBuilder(args);
    }

    public IServiceCollection Services => _appBuilder.Services;

    // ------------------------------------------------------------------------
    // Fluent registration methods
    // ------------------------------------------------------------------------

    public IAgentHostBuilder AddEngine(string engineId, Func<IEngine> factory)
    {
        if (string.IsNullOrWhiteSpace(engineId))
            throw new ArgumentException("engineId is required", nameof(engineId));

        Services.AddSingleton<IEngine>(_ => factory());
        return this;
    }

    public IAgentHostBuilder AddRunner(string engineId, Func<IRunner> factory)
    {
        if (string.IsNullOrWhiteSpace(engineId))
            throw new ArgumentException("engineId is required", nameof(engineId));

        Services.AddSingleton<IRunner>(_ =>
        {
            var runner = factory();
            runner.EngineId = engineId;
            return runner;
        });
        return this;
    }

    public IAgentHostBuilder AddAgent(string agentId, Func<IAgent> factory)
    {
        if (string.IsNullOrWhiteSpace(agentId))
            throw new ArgumentException("agentId is required", nameof(agentId));

        Services.AddSingleton<IAgent>(_ => factory());
        return this;
    }

    public IAgentHostBuilder Attach(string agentId, string engineId, PolicySet? overrides = null)
    {
        if (string.IsNullOrWhiteSpace(agentId))
            throw new ArgumentException("agentId is required", nameof(agentId));
        if (string.IsNullOrWhiteSpace(engineId))
            throw new ArgumentException("engineId is required", nameof(engineId));

        Services.AddSingleton(new Attachment(agentId, engineId, overrides));
        return this;
    }

    public IAgentHostBuilder WithKernelDefaults(PolicySet defaults)
    {
        if (defaults is null)
            throw new ArgumentNullException(nameof(defaults));

        Services.AddSingleton(defaults);
        return this;
    }

    public AgentHostBuilder WithKernelConcurrency(int workerCount)
    {
        if (workerCount < 1)
            throw new ArgumentOutOfRangeException(nameof(workerCount), "Worker count must be >= 1.");

        Services.AddSingleton(new KernelConcurrency(workerCount));
        return this;
    }

    public IAgentHostBuilder WithKernel(Func<IKernelFactory> factory)
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        Services.AddSingleton(typeof(IKernelFactory), _ => factory());
        return this;
    }

    public IAgentHostBuilder EnableDashboard(int port = 6060)
    {
        // Register the port value as a singleton record
        Services.AddSingleton(new DashboardPort(port));

        // Register the hosted service; the service itself will resolve the inspector lazily
        Services.AddHostedService<ObservabilityDashboardService>();

        return this;
    }

    // ------------------------------------------------------------------------
    // Build
    // ------------------------------------------------------------------------

    /// <summary>
    /// Builds the host and returns an <see cref="AgentHost"/> wrapper.
    /// </summary>
    public IAgentHost Build()
    {
        // Register console logging provider
        _appBuilder.Services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSimpleConsole();
        });

        // Register AgentHostConfig as DI-assembled snapshot
        Services.AddSingleton<AgentHostConfig>();

        // Register KernelRuntime orchestrator (BackgroundService)
        Services.AddHostedService<KernelRuntime>();

        var host = _appBuilder.Build();
        return new AgentHost(host);
    }
}
