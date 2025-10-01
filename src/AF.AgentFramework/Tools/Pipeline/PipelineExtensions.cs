using AgentFramework.Tools.Pipeline.Defaults;
using AgentFramework.Tools.Registry;

namespace AgentFramework.Tools.Pipeline;

/// <summary>
/// Extension methods for composing tool pipeline components.
/// </summary>
public static class PipelineExtensions
{
    /// <summary>
    /// Creates a pipeline with the default stage implementations:
    /// pass-through validation, allow-all authorization, timeout-only policy,
    /// not-implemented executor, and pass-through postprocessor.
    /// </summary>
    public static ToolPipeline WithDefaultStages(this IToolRegistry registry)
    {
        return new ToolPipeline(
            registry,
            new PassThroughInputValidator(),
            new AllowAllAuthorizer(),
            new TimeoutOnlyPolicyApplier(),
            new NotImplementedExecutor(),
            new PassThroughPostprocessor());
    }
}