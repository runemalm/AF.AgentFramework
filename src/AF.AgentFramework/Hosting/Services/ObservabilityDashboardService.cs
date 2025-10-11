using AgentFramework.Hosting.Observability;
using AgentFramework.Kernel.Diagnostics;

namespace AgentFramework.Hosting.Services;

public sealed class ObservabilityDashboardService : IAgentHostService
{
    private readonly int _port;
    private ObservabilityServer? _server;

    public ObservabilityDashboardService(int port = 6060)
    {
        _port = port;
    }

    public async Task StartAsync(IAgentHostContext context, CancellationToken ct)
    {
        if (context.Kernel is IKernelInspector inspector)
        {
            _server = new ObservabilityServer(_port, inspector);
            await _server.StartAsync(ct);
        }
    }

    public async Task StopAsync(CancellationToken ct)
    {
        if (_server is not null)
            await _server.StopAsync(ct);
    }
}
