using Microsoft.Extensions.DependencyInjection;
using AgentFramework.Engines;
using AgentFramework.Hosting.Services;
using AgentFramework.Kernel;

namespace AgentFramework.Hosting;

/// <summary>
/// Composes Kernel + Engines + Runners + Agents and manages lifecycle.
/// </summary>
public sealed class AgentHost : IAgentHost
{
    private readonly AgentHostConfig _config;
    private readonly IServiceProvider _services;
    private readonly IKernelFactory _kernelFactory;

    private IKernel? _kernel;
    private readonly Dictionary<string, IEngine> _engines = new(StringComparer.Ordinal);
    private AgentCatalog? _catalog;
    private readonly List<IAgentHostService> _hostServices = new();

    public AgentHost(AgentHostConfig config, IServiceProvider services)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _kernelFactory = _services.GetRequiredService<IKernelFactory>();
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        // 1) Instantiate agent singletons
        var agents = new Dictionary<string, IAgent>(StringComparer.Ordinal);
        foreach (var reg in _config.Agents)
        {
            if (string.IsNullOrWhiteSpace(reg.AgentId))
                throw new InvalidOperationException("Agent registration has empty AgentId.");
            if (agents.ContainsKey(reg.AgentId))
                throw new InvalidOperationException($"Duplicate AgentId '{reg.AgentId}'.");
            var instance = reg.Factory()
                ?? throw new InvalidOperationException($"Agent factory for '{reg.AgentId}' returned null.");
            if (!string.Equals(instance.Id, reg.AgentId, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    $"Agent instance Id '{instance.Id}' does not match registration '{reg.AgentId}'.");
            agents.Add(reg.AgentId, instance);
        }
        _catalog = new AgentCatalog(agents);

        // 2) Create Kernel
        var defaults = _config.KernelDefaults
                       ?? throw new InvalidOperationException("Kernel defaults not provided. Call WithKernelDefaults().");

        var bindings = _config.Attachments.Select(att => new AttachmentBinding
        {
            AgentId = att.AgentId,
            EngineId = att.EngineId,
            Policies = att.Overrides ?? defaults
        }).ToList();

        _kernel = _kernelFactory.Create(new KernelOptions
        {
            Agents = _catalog,
            Defaults = defaults,
            Bindings = bindings,
            WorkerCount = _config.WorkerCount
        });

        // 3) Instantiate engines
        foreach (var engReg in _config.Engines)
        {
            if (string.IsNullOrWhiteSpace(engReg.EngineId))
                throw new InvalidOperationException("Engine registration has empty EngineId.");
            if (_engines.ContainsKey(engReg.EngineId))
                throw new InvalidOperationException($"Duplicate EngineId '{engReg.EngineId}'.");
            var engine = engReg.Factory()
                ?? throw new InvalidOperationException($"Engine factory for '{engReg.EngineId}' returned null.");
            if (!string.Equals(engine.Id, engReg.EngineId, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    $"Engine instance Id '{engine.Id}' does not match registration '{engReg.EngineId}'.");
            _engines.Add(engReg.EngineId, engine);
        }

        // 4) Bind kernel & attach runners
        foreach (var e in _engines.Values)
        {
            e.BindKernel(_kernel);
            foreach (var r in _config.Runners.Where(r => r.EngineId == e.Id))
            {
                var runner = r.Factory()
                    ?? throw new InvalidOperationException($"Runner factory for engine '{e.Id}' returned null.");
                e.AddRunner(runner);
            }
        }

        // 5) Validate attachments (agent/engine exist) and pass to engines
        foreach (var att in _config.Attachments)
        {
            if (!_engines.ContainsKey(att.EngineId))
                throw new InvalidOperationException($"Attachment references unknown engine '{att.EngineId}'.");
            if (_catalog.ListIds().All(id => !string.Equals(id, att.AgentId, StringComparison.Ordinal)))
                throw new InvalidOperationException($"Attachment references unknown agent '{att.AgentId}'.");
        }
        foreach (var group in _config.Attachments.GroupBy(a => a.EngineId))
        {
            var list = group.Select(a => a.AgentId).Distinct(StringComparer.Ordinal).ToList();
            _engines[group.Key].SetAttachments(list);
        }

        // 6) Start kernel then engines
        await _kernel.StartAsync(ct).ConfigureAwait(false);
        foreach (var e in _engines.Values)
        {
            await e.StartAsync(ct).ConfigureAwait(false);
        }
        
        // 7) Start host services (e.g. dashboard)
        var ctx = new AgentHostContext(_kernel, _engines, _services);
        foreach (var factory in _config.HostServices.Factories)
        {
            var svc = factory();
            _hostServices.Add(svc);
            Console.WriteLine($"[Host] Starting host service {svc.GetType().Name}…");
            await svc.StartAsync(ctx, ct);
        }
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        // Stop host services first
        foreach (var svc in _hostServices.AsEnumerable().Reverse())
        {
            Console.WriteLine($"[Host] Stopping host service {svc.GetType().Name}…");
            await svc.StopAsync(ct);
        }

        // Stop engines, then kernel
        foreach (var e in _engines.Values.Reverse())
            await e.StopAsync(ct).ConfigureAwait(false);
        if (_kernel is not null)
            await _kernel.StopAsync(ct).ConfigureAwait(false);
    }
}