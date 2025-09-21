using AgentFramework.Kernel.Policies;

namespace AgentFramework.Hosting;

/// <summary>
/// Internal bag of registrations; produced by AgentHostBuilder.
/// </summary>
public sealed class AgentHostConfig
{
    public List<EngineRegistration> Engines { get; } = new();
    public List<RunnerRegistration> Runners { get; } = new();
    public List<AgentRegistration> Agents { get; } = new();
    public List<Attachment> Attachments { get; } = new();
    public PolicySet? KernelDefaults { get; set; }
}
