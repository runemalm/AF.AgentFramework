namespace AgentFramework.Tools.Integration;

/// <summary>
/// Factory delegate used by the host to create per-agent tool facades.
/// </summary>
public delegate AgentContextTools ToolSubsystemFactory(string agentId);