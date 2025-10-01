namespace AgentFramework.Tools.Contracts;

/// <summary>
/// Utilities for working with correlation/causation identifiers.
/// </summary>
public static class Correlation
{
    /// <summary>
    /// Generates a compact correlation id suitable for logs and tracing.
    /// </summary>
    public static string NewId() => Guid.NewGuid().ToString("N");

    /// <summary>
    /// Returns <paramref name="existing"/> if it is non-empty; otherwise generates a new id.
    /// </summary>
    public static string Ensure(string? existing) => string.IsNullOrWhiteSpace(existing) ? NewId() : existing!;
}