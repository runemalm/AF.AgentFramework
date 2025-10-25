using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AgentFramework.Engines;
using AgentFramework.Kernel;
using AgentFramework.Runners;

namespace AgentFramework.Hosting;

/// <summary>
/// BackgroundService responsible for orchestrating the lifecycle
/// of the kernel, engines, and runners in deterministic order.
/// </summary>
public sealed class KernelRuntime : BackgroundService
{
    private readonly AgentHostConfig _config;
    private readonly IKernelFactory _kernelFactory;
    private readonly ILogger<KernelRuntime> _logger;

    private IKernel? _kernel;
    private readonly List<IEngine> _engines = new();
    private readonly List<IRunner> _runners = new();

    public KernelRuntime(
        AgentHostConfig config,
        IKernelFactory kernelFactory,
        ILogger<KernelRuntime> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _kernelFactory = kernelFactory ?? throw new ArgumentNullException(nameof(kernelFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Starts the kernel, engines, and runners in the correct order.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("[Runtime] Starting AgentFramework kernel runtime…");

            // 1) Initialize kernel
            _kernel = _kernelFactory.Create(new KernelOptions
            {
                Agents = new AgentCatalog(_config.Agents.ToDictionary(a => a.Id, a => a)),
                Defaults = _config.KernelDefaults,
                Bindings = _config.Attachments.Select(att => new AttachmentBinding
                {
                    AgentId = att.AgentId,
                    EngineId = att.EngineId,
                    Policies = att.Overrides ?? _config.KernelDefaults
                }).ToList(),
                WorkerCount = _config.WorkerCount
            });

            // 2) Start kernel
            await _kernel.StartAsync(stoppingToken).ConfigureAwait(false);

            // 3) Bind kernel to engines
            foreach (var engine in _config.Engines)
            {
                engine.BindKernel(_kernel);
                _engines.Add(engine);
            }
            
            // 4) Attach runners to engines (before engines start)
            foreach (var runner in _config.Runners)
            {
                var engine = _config.Engines.FirstOrDefault(e => e.Id == runner.EngineId);
                if (engine is null)
                {
                    _logger.LogWarning("[Runtime] Runner '{runner}' could not find engine '{engineId}'.",
                        runner.Name, runner.EngineId);
                    continue;
                }

                engine.AddRunner(runner);
                _runners.Add(runner);

                _logger.LogInformation("[Runtime] Attached runner '{runner}' to engine '{engine}'.",
                    runner.Name, engine.Id);
            }

            // 5) Set attachments (agents → engines)
            foreach (var engine in _config.Engines)
            {
                var attachedAgents = _config.Attachments
                    .Where(a => a.EngineId == engine.Id)
                    .Select(a => a.AgentId)
                    .ToList();

                engine.SetAttachments(attachedAgents);
            }

            // 6) Start engines (will start runners internally)
            foreach (var engine in _engines)
            {
                await engine.StartAsync(stoppingToken).ConfigureAwait(false);
                _logger.LogInformation("[Runtime] Engine '{engine}' started.", engine.Id);
            }

            _logger.LogInformation("[Runtime] Kernel runtime started successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Runtime] Failed to start kernel runtime.");
            throw;
        }
    }

    /// <summary>
    /// Stops runners, engines, and kernel in reverse order.
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[Runtime] Stopping AgentFramework runtime…");

        foreach (var runner in _runners.AsEnumerable().Reverse())
        {
            try
            {
                await runner.StopAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("[Runtime] Runner '{runner}' stopped.", runner.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Runtime] Failed stopping runner '{runner}'.", runner.GetType().Name);
            }
        }

        foreach (var engine in _engines.AsEnumerable().Reverse())
        {
            try
            {
                await engine.StopAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("[Runtime] Engine '{engine}' stopped.", engine.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Runtime] Failed stopping engine '{engine}'.", engine.Id);
            }
        }

        if (_kernel is not null)
        {
            try
            {
                await _kernel.StopAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("[Runtime] Kernel stopped.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Runtime] Failed stopping kernel: {error}", ex.Message);
            }
        }

        _logger.LogInformation("[Runtime] Runtime shutdown complete.");
    }
}
