using AgentFramework.Kernel;
using AgentFramework.Kernel.Policies;
using AgentFramework.Engines;
using AgentFramework.Runners;
using AgentFramework.Hosting.Services;
using AgentFramework.Kernel.Features;
using AgentFramework.Kernel.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFramework.Hosting;

public sealed class AgentHostBuilder : IAgentHostBuilder, IAgentCapabilityRegistrar
{
    private readonly AgentHostConfig _config = new();
    private readonly ServiceCollection _services = new();

    public static IAgentHostBuilder Create()
    {
        var builder = new AgentHostBuilder();
        builder.Services.AddSingleton<IStimulusRouter, BroadcastStimulusRouter>();
        builder.Services.AddSingleton<FilteredStimulusRouter>();
        builder.Services.AddSingleton<IPerceptFilterRegistry, InMemoryPerceptFilterRegistry>();
        return builder;
    }
    
    public IServiceCollection Services => _services;
    
    // Agent capability registration
    void IAgentCapabilityRegistrar.RegisterCapability(Type interfaceType, Type implementationType)
    {
        if (!typeof(IAgentFeature).IsAssignableFrom(interfaceType))
            throw new ArgumentException($"'{interfaceType.Name}' must implement IAgentFeature.", nameof(interfaceType));

        if (!typeof(IAgentFeature).IsAssignableFrom(implementationType))
            throw new ArgumentException($"'{implementationType.Name}' must implement IAgentFeature.", nameof(implementationType));

        _config.FeatureRegistrations.Add(new FeatureRegistration
        {
            InterfaceType = interfaceType,
            ImplementationType = implementationType
        });
    }
    
    public IAgentHostBuilder AddEngine(string engineId, Func<IEngine> engineFactory)
    {
        if (engineId is null) throw new ArgumentNullException(nameof(engineId));
        if (engineFactory is null) throw new ArgumentNullException(nameof(engineFactory));

        _config.Engines.Add(new EngineRegistration
        {
            EngineId = engineId,
            Factory = _ => engineFactory()
        });
        return this;
    }

    public IAgentHostBuilder AddEngine(string engineId, Func<IServiceProvider, IEngine> engineFactory)
    {
        if (engineId is null) throw new ArgumentNullException(nameof(engineId));
        if (engineFactory is null) throw new ArgumentNullException(nameof(engineFactory));

        _config.Engines.Add(new EngineRegistration
        {
            EngineId = engineId,
            Factory = engineFactory
        });
        return this;
    }

    public IAgentHostBuilder AddRunner(string engineId, Func<IRunner> runnerFactory)
    {
        if (string.IsNullOrWhiteSpace(engineId)) throw new ArgumentException("engineId is required", nameof(engineId));
        _config.Runners.Add(new RunnerRegistration { EngineId = engineId, Factory = runnerFactory ?? throw new ArgumentNullException(nameof(runnerFactory)) });
        return this;
    }

    public IAgentHostBuilder AddAgent(string agentId, Func<IAgent> agentFactory)
    {
        if (string.IsNullOrWhiteSpace(agentId)) throw new ArgumentException("agentId is required", nameof(agentId));
        _config.Agents.Add(new AgentRegistration { AgentId = agentId, Factory = agentFactory ?? throw new ArgumentNullException(nameof(agentFactory)) });
        return this;
    }

    public IAgentHostBuilder Attach(string agentId, string engineId, PolicySet? overrides = null)
    {
        if (string.IsNullOrWhiteSpace(agentId)) throw new ArgumentException("agentId is required", nameof(agentId));
        if (string.IsNullOrWhiteSpace(engineId)) throw new ArgumentException("engineId is required", nameof(engineId));
        _config.Attachments.Add(new Attachment { AgentId = agentId, EngineId = engineId, Overrides = overrides });
        return this;
    }

    public IAgentHostBuilder WithKernelDefaults(PolicySet defaults)
    {
        _config.KernelDefaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
        return this;
    }
    
    public AgentHostBuilder WithKernelConcurrency(int workerCount)
    {
        if (workerCount < 1)
            throw new ArgumentOutOfRangeException(nameof(workerCount), "Worker count must be at least 1.");

        _config.WorkerCount = workerCount;
        return this;
    }
    
    public IAgentHostBuilder WithKernel(Func<IKernelFactory> factory)
    {
        if (factory is null) throw new ArgumentNullException(nameof(factory));
        _services.AddSingleton(typeof(IKernelFactory), _ => factory());
        return this;
    }
    
    public IAgentHostBuilder AddHostService(Func<IAgentHostService> factory)
    {
        _config.HostServices.Factories.Add(factory);
        return this;
    }

    public IAgentHostBuilder EnableDashboard(int port = 6060)
    {
        return AddHostService(() => new ObservabilityDashboardService(port));
    }

    public IAgentHost Build()
    {
        var serviceProvider = _services.BuildServiceProvider(validateScopes: false);
        return new AgentHost(_config, serviceProvider);
    }
}
