using Microsoft.Extensions.DependencyInjection;
using AgentFramework.Hosting;
using AgentFramework.Tools.Runtime;

namespace AgentFramework.Tools.Integration;

/// <summary>
/// Extension methods for registering local tool implementations on the host builder.
/// </summary>
public static class AgentHostBuilderLocalToolsExtensions
{
    /// <summary>
    /// Registers a local tool implementation to be published and executable through the pipeline.
    /// </summary>
    public static IAgentHostBuilder AddLocalTool<TTool>(this IAgentHostBuilder builder)
        where TTool : class, ILocalTool
    {
        builder.Services.AddSingleton<ILocalTool, TTool>();
        return builder;
    }
}
