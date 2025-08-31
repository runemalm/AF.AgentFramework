namespace AgentFramework.Kernel;

public readonly record struct AgentId(string Value);

public record Message(string Role, string Content);

public readonly record struct ToolCall(string Name, string ArgumentsJson);

public readonly record struct ToolResult(string Name, string ResultJson);

public record Decision(Message? Reply, IReadOnlyList<ToolCall> ToolCalls);
