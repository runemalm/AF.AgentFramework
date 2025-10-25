namespace AgentFramework.Kernel.Features;

/// <summary>
/// Represents a registered agent capability mapping from interface to implementation.
/// </summary>
public sealed class FeatureRegistration
{
    public required Type InterfaceType { get; init; }
    public required Type ImplementationType { get; init; }

    public override string ToString() => $"{InterfaceType.Name} → {ImplementationType.Name}";
}
