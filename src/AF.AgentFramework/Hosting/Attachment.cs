using AgentFramework.Kernel.Policies;

namespace AgentFramework.Hosting;

/// <summary>
/// Represents a single agent→engine attachment.
/// </summary>
public sealed record Attachment(string AgentId, string EngineId, PolicySet? Overrides = null);
