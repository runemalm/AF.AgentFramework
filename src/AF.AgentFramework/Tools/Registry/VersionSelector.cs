namespace AgentFramework.Tools.Registry;

/// <summary>
/// Selects a concrete version from a set of available versions, given an optional range.
/// Current implementation:
/// - If range is null/empty: return the highest (lexicographically SemVer-aware) version.
/// - If range is exact (e.g., "1.2.3"): return that version if available.
/// - If range starts with "^" (caret range): select the highest version with the same major as the base.
/// - Otherwise, attempt exact match; if none, throw.
/// NOTE: This is a minimal selector; replace with a full SemVer/range lib if needed.
/// </summary>
public static class VersionSelector
{
    public static string Select(IReadOnlyCollection<string> available, string? range)
    {
        if (available == null || available.Count == 0)
            throw new InvalidOperationException("No available versions to select from.");

        var parsed = available
            .Select(v => (raw: v, sem: ParseSemVer(v)))
            .OrderByDescending(t => t.sem)
            .ToList();

        if (string.IsNullOrWhiteSpace(range))
        {
            // Highest available
            return parsed.First().raw;
        }

        range = range.Trim();

        // Exact match
        if (!range.Contains('^') && !range.Contains('*'))
        {
            var exact = parsed.FirstOrDefault(t => string.Equals(t.raw, range, StringComparison.OrdinalIgnoreCase));
            if (exact.raw != null) return exact.raw;

            // If it's a plain "major.minor" without patch, try best match within that minor
            if (TryParseSemVer(range, out var wanted) && wanted.patch < 0)
            {
                var match = parsed
                    .Where(t => t.sem.major == wanted.major && t.sem.minor == wanted.minor)
                    .FirstOrDefault();
                if (match.raw != null) return match.raw;
            }

            throw new InvalidOperationException($"No version matches the requested range '{range}'.");
        }

        // Caret ranges like ^1.2.0 -> allow >=1.2.0 <2.0.0
        if (range.StartsWith("^", StringComparison.Ordinal))
        {
            var baseStr = range.Substring(1);
            if (!TryParseSemVer(baseStr, out var baseVer) || baseVer.major < 0)
                throw new InvalidOperationException($"Invalid caret range '{range}'.");

            var candidate = parsed
                .Where(t => t.sem.major == baseVer.major &&
                            (t.sem.minor > baseVer.minor ||
                             (t.sem.minor == baseVer.minor && t.sem.patch >= Math.Max(0, baseVer.patch))))
                .FirstOrDefault();

            if (candidate.raw != null) return candidate.raw;

            throw new InvalidOperationException($"No version satisfies caret range '{range}'.");
        }

        // Basic wildcard: "1.*" -> highest with major 1; "1.2.*" -> highest with major 1 minor 2
        if (range.Contains('*'))
        {
            if (!TryParseWildcard(range, out var wantedMajor, out var wantedMinor))
                throw new InvalidOperationException($"Invalid wildcard range '{range}'.");

            var candidate = parsed
                .Where(t => t.sem.major == wantedMajor &&
                            (wantedMinor < 0 || t.sem.minor == wantedMinor))
                .FirstOrDefault();

            if (candidate.raw != null) return candidate.raw;

            throw new InvalidOperationException($"No version satisfies wildcard range '{range}'.");
        }

        throw new InvalidOperationException($"Unsupported version range syntax '{range}'.");
    }

    private static (int major, int minor, int patch) ParseSemVer(string v)
    {
        if (!TryParseSemVer(v, out var res))
            return (-1, -1, -1);
        return res;
    }

    private static bool TryParseSemVer(string v, out (int major, int minor, int patch) res)
    {
        res = (-1, -1, -1);
        if (string.IsNullOrWhiteSpace(v)) return false;

        // Strip pre-release/build metadata if present
        var core = v.Split('-', '+')[0];
        var parts = core.Split('.');
        if (parts.Length == 0) return false;

        bool okMajor = int.TryParse(parts[0], out var major);
        int minor = -1, patch = -1;
        if (parts.Length > 1) int.TryParse(parts[1], out minor);
        if (parts.Length > 2) int.TryParse(parts[2], out patch);

        if (!okMajor) return false;

        res = (major, minor, patch);
        return true;
    }

    private static bool TryParseWildcard(string range, out int major, out int minor)
    {
        major = -1; minor = -1;
        var core = range.Replace("*", "").TrimEnd('.');
        var parts = core.Split('.', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0) return false;
        if (!int.TryParse(parts[0], out major)) return false;

        if (range.EndsWith(".*", StringComparison.Ordinal))
        {
            // "1.*" or "1.2.*"
            if (parts.Length == 2 && int.TryParse(parts[1], out var m))
            {
                minor = m;
                return true;
            }
            if (parts.Length == 1)
            {
                minor = -1;
                return true;
            }
            return false;
        }

        // If no trailing *, treat as exact major/minor
        if (parts.Length == 2 && int.TryParse(parts[1], out var exactMinor))
        {
            minor = exactMinor;
            return true;
        }

        return false;
    }
}
