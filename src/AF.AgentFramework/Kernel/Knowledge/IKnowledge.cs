namespace AgentFramework.Kernel.Knowledge;

/// <summary>
/// Minimal, pluggable Knowledge (the "K" in MAPE-K).
/// Keeps it intentionally small for Iteration 1.
/// 
/// Design notes:
/// - Keys are arbitrary strings; use ":" to emulate scopes (e.g., "observe:cpu", "plan:intent").
/// - Values are typed; caller is responsible for consistent types per key.
/// - Thread-safe implementations are recommended.
/// - Version is a monotonic counter useful for cheap change detection.
/// </summary>
public interface IKnowledge
{
    /// <summary>Monotonically increasing version; bump on any write/remove.</summary>
    long Version { get; }

    /// <summary>Try to read a value of type T from the store.</summary>
    bool TryGet<T>(string key, out T? value);

    /// <summary>Get a value or a provided default if missing.</summary>
    T? GetOrDefault<T>(string key, T? defaultValue = default);

    /// <summary>Set/overwrite a value.</summary>
    void Set<T>(string key, T value);

    /// <summary>Remove a value if present; returns true if something was removed.</summary>
    bool Remove(string key);

    /// <summary>Clear all entries.</summary>
    void Clear();
}