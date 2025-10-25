namespace AgentFramework.Kernel.Features;

/// <summary>
/// Represents the set of features currently attached to an <see cref="IAgentContext"/>.
/// Provides lookup and enumeration over feature interfaces and their active implementations.
/// </summary>
public interface IAgentFeatureCollection : IEnumerable<IAgentFeature>
{
    /// <summary>
    /// Adds or replaces a feature instance under the specified interface type.
    /// </summary>
    /// <param name="interfaceType">The interface type the feature implements.</param>
    /// <param name="feature">The feature instance to register.</param>
    void Add(Type interfaceType, IAgentFeature feature);

    /// <summary>
    /// Gets the feature implementation of the specified interface type, or <c>null</c> if not present.
    /// </summary>
    /// <typeparam name="TInterface">The feature interface type to retrieve.</typeparam>
    /// <returns>The feature instance, or <c>null</c> if not found.</returns>
    TInterface? Get<TInterface>() where TInterface : class, IAgentFeature;

    /// <summary>
    /// Checks whether a feature of the specified interface type exists in this collection.
    /// </summary>
    /// <typeparam name="TInterface">The feature interface type to check for.</typeparam>
    /// <returns><c>true</c> if a feature implementing <typeparamref name="TInterface"/> is present.</returns>
    bool Contains<TInterface>() where TInterface : class, IAgentFeature;

    /// <summary>
    /// Gets all feature interface types currently registered in this collection.
    /// </summary>
    IEnumerable<Type> Keys { get; }
}
