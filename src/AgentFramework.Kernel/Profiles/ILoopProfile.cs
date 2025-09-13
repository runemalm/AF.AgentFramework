namespace AgentFramework.Kernel.Profiles;

/// <summary>
/// Provides an ordered, static list of steps to run for a single kernel tick.
/// </summary>
public interface ILoopProfile
{
    string Name { get; }
    IReadOnlyList<LoopStepDescriptor> GetSteps();
}
