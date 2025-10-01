using AgentFramework.Tools.Contracts;

namespace AgentFramework.Tools.Pipeline;

/// <summary>
/// Performs output validation, redaction, or transformation after execution,
/// before the final result is returned.
/// </summary>
public interface IPostprocessor
{
    PostprocessOutcome Postprocess(ToolResult result, object? outputSchema);
}

/// <summary>
/// Outcome of post-processing. Typically wraps the final ToolResult.
/// </summary>
public sealed record class PostprocessOutcome
{
    public ToolResult Result { get; init; } = new();
}