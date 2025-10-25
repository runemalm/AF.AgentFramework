using AgentFramework.Engines;
using AgentFramework.Kernel;
using AgentFramework.Kernel.Policies;
using AgentFramework.Runners;

namespace AgentFramework.Hosting;

/// <summary>
/// Represents the assembled topology of an agent host —
/// including kernel configuration, engines, runners, agents, and attachments.
/// Built automatically by the DI container via constructor injection.
/// </summary>
public sealed class AgentHostConfig
{
    public IReadOnlyList<IEngine> Engines { get; }
    public IReadOnlyList<IRunner> Runners { get; }
    public IReadOnlyList<IAgent> Agents { get; }
    public IReadOnlyList<Attachment> Attachments { get; }

    public IKernelFactory KernelFactory { get; }
    public PolicySet KernelDefaults { get; }
    public int WorkerCount { get; }

    public AgentHostConfig(
        IEnumerable<IEngine> engines,
        IEnumerable<IRunner> runners,
        IEnumerable<IAgent> agents,
        IEnumerable<Attachment> attachments,
        IKernelFactory kernelFactory,
        PolicySet kernelDefaults,
        KernelConcurrency concurrency)
    {
        Engines = engines?.ToList() ?? new List<IEngine>();
        Runners = runners?.ToList() ?? new List<IRunner>();
        Agents = agents?.ToList() ?? new List<IAgent>();
        Attachments = attachments?.ToList() ?? new List<Attachment>();
        KernelFactory = kernelFactory ?? throw new ArgumentNullException(nameof(kernelFactory));
        KernelDefaults = kernelDefaults ?? throw new ArgumentNullException(nameof(kernelDefaults));
        WorkerCount = concurrency?.WorkerCount ?? Math.Max(1, Environment.ProcessorCount / 2);;

        Validate();
    }

    private void Validate()
    {
        var duplicateAgents = Agents.GroupBy(a => a.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicateAgents.Count > 0)
            throw new InvalidOperationException($"Duplicate agent IDs: {string.Join(", ", duplicateAgents)}");

        var duplicateEngines = Engines.GroupBy(e => e.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicateEngines.Count > 0)
            throw new InvalidOperationException($"Duplicate engine IDs: {string.Join(", ", duplicateEngines)}");

        foreach (var att in Attachments)
        {
            if (!Engines.Any(e => e.Id == att.EngineId))
                throw new InvalidOperationException($"Attachment references unknown engine '{att.EngineId}'.");

            if (!Agents.Any(a => a.Id == att.AgentId))
                throw new InvalidOperationException($"Attachment references unknown agent '{att.AgentId}'.");
        }
    }
}
