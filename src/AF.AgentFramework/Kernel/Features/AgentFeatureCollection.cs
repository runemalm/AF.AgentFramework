using System.Collections;

namespace AgentFramework.Kernel.Features;

/// <summary>
/// Default in-memory implementation of <see cref="IAgentFeatureCollection"/>.
/// Maintains the set of active features attached to a specific agent context.
/// </summary>
internal sealed class AgentFeatureCollection : IAgentFeatureCollection
{
    private readonly Dictionary<Type, IAgentFeature> _features = new();

    /// <summary>
    /// Adds or replaces a feature instance under the specified interface type.
    /// </summary>
    public void Add(Type interfaceType, IAgentFeature feature)
    {
        ArgumentNullException.ThrowIfNull(interfaceType);
        ArgumentNullException.ThrowIfNull(feature);

        if (!interfaceType.IsAssignableFrom(feature.GetType()))
        {
            throw new ArgumentException(
                $"Feature instance of type '{feature.GetType().Name}' does not implement '{interfaceType.Name}'.",
                nameof(feature));
        }

        _features[interfaceType] = feature;
    }

    /// <summary>
    /// Gets the feature of the specified interface type, or <c>null</c> if none is present.
    /// </summary>
    public TInterface? Get<TInterface>()
        where TInterface : class, IAgentFeature
        => _features.TryGetValue(typeof(TInterface), out var f) ? (TInterface)f : null;

    /// <summary>
    /// Checks whether a feature of the given interface type exists in this collection.
    /// </summary>
    public bool Contains<TInterface>() where TInterface : class, IAgentFeature
        => _features.ContainsKey(typeof(TInterface));

    /// <summary>
    /// Returns all registered feature interface types.
    /// </summary>
    public IEnumerable<Type> Keys => _features.Keys;

    /// <summary>
    /// Returns an enumerator over all feature instances in this collection.
    /// </summary>
    public IEnumerator<IAgentFeature> GetEnumerator() => _features.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString()
        => $"Features[{string.Join(", ", _features.Keys.Select(t => t.Name))}]";
}
