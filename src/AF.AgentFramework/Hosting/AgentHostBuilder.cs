using AgentFramework.Kernel;
using AgentFramework.Kernel.Policies;
using AgentFramework.Engines;
using AgentFramework.Runners;

namespace AgentFramework.Hosting;

/// <summary>
/// Minimal, compiling builder. Stores registrations; Build() returns a Noop host for now.
/// </summary>
public sealed class AgentHostBuilder : IAgentHostBuilder
{
    private readonly AgentHostConfig _config = new();

    public static IAgentHostBuilder Create() => new AgentHostBuilder();

    public IAgentHostBuilder AddEngine(string engineId, Func<IEngine> engineFactory)
    {
        if (string.IsNullOrWhiteSpace(engineId)) throw new ArgumentException("engineId is required", nameof(engineId));
        _config.Engines.Add(new EngineRegistration { EngineId = engineId, Factory = engineFactory ?? throw new ArgumentNullException(nameof(engineFactory)) });
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

    public IAgentHost Build()
    {
        // TODO: replace NoopAgentHost with real host that composes Kernel + Engines + Runners.
        return new AgentHost(_config);
    }
}
