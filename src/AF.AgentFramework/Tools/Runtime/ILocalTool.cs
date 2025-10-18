using AgentFramework.Tools.Contracts;

namespace AgentFramework.Tools.Runtime;

/// <summary>
/// Represents a locally executable tool implementation.
/// Local tools are registered with metadata and can be invoked through the tool pipeline.
/// </summary>
public interface ILocalTool
{
    /// <summary>
    /// Fully qualified name (e.g., "local::echo").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Concrete version string (e.g., "1.0.0").
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Declarative contract describing input/output schema and effect semantics.
    /// </summary>
    ToolContract Contract { get; }

    /// <summary>
    /// Executes the tool locally.
    /// </summary>
    Task<ToolResult> ExecuteAsync(object? input, CancellationToken cancellationToken);
}
