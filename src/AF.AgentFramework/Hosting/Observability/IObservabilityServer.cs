namespace AgentFramework.Hosting.Observability;

public interface IObservabilityServer
{
    int Port { get; }
    public Uri BaseAddress { get; }
}
