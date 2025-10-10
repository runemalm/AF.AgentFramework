using System.Collections.Concurrent;

namespace AgentFramework.Kernel.Knowledge;

/// <summary>
/// Simple in-memory Knowledge implementation.
/// - Thread-safe via ConcurrentDictionary.
/// - Version increments on any mutation (Set/Remove/Clear).
/// </summary>
public sealed class InMemoryKnowledge : IKnowledge
{
    private readonly ConcurrentDictionary<string, object?> _data = new();
    private long _version;

    public long Version => Interlocked.Read(ref _version);

    public bool TryGet<T>(string key, out T? value)
    {
        if (_data.TryGetValue(key, out var obj) && obj is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    public T? GetOrDefault<T>(string key, T? defaultValue = default)
        => TryGet<T>(key, out var value) ? value : defaultValue;

    public void Set<T>(string key, T value)
    {
        _data[key] = value;
        Interlocked.Increment(ref _version);
    }

    public bool Remove(string key)
    {
        var removed = _data.TryRemove(key, out _);
        if (removed)
            Interlocked.Increment(ref _version);
        return removed;
    }

    public void Clear()
    {
        if (_data.IsEmpty) return;
        _data.Clear();
        Interlocked.Increment(ref _version);
    }
}

