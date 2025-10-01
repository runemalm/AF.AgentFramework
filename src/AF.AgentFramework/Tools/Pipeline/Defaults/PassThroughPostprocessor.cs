using AgentFramework.Tools.Contracts;

namespace AgentFramework.Tools.Pipeline.Defaults;

/// <summary>
/// Returns the result unchanged. Placeholder for output validation/redaction.
/// </summary>
public sealed class PassThroughPostprocessor : IPostprocessor
{
    public PostprocessOutcome Postprocess(ToolResult result, object? outputSchema)
        => new() { Result = result };
}