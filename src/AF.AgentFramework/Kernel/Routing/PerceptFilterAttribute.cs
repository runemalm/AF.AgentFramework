namespace AgentFramework.Kernel.Routing;

/// <summary>
/// Declares a static percept filter (topic pattern) for an agent.
/// These are automatically registered in <see cref="IPerceptFilterRegistry"/> when the host starts.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class PerceptFilterAttribute : Attribute
{
    public string Pattern { get; }

    public PerceptFilterAttribute(string pattern)
    {
        Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
    }
}
