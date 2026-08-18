using FarlaTweaks.Core.Models;

namespace FarlaTweaks.Core.Compatibility;

public sealed class CompatibilityEngine
{
    public CompatibilityResult Evaluate(
        TweakDefinition tweak,
        SystemProfile profile,
        IEnumerable<string>? selectedTweakIds = null)
    {
        var reasons = new List<string>();
        var warnings = new List<string>();
        var conflicts = new List<string>();
        var selected = selectedTweakIds?.ToHashSet(StringComparer.OrdinalIgnoreCase)
                       ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(tweak.RequiredOsFamily) &&
            !string.Equals(tweak.RequiredOsFamily, profile.OsFamily, StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add($"Requires OS family '{tweak.RequiredOsFamily}'.");
        }

        foreach (var dependency in tweak.Dependencies)
        {
            if (!profile.Capabilities.Contains(dependency, StringComparer.OrdinalIgnoreCase))
                reasons.Add($"Required capability '{dependency}' is not detected.");
        }

        foreach (var conflict in tweak.Conflicts)
        {
            if (selected.Contains(conflict))
                conflicts.Add($"Conflicts with selected tweak '{conflict}'.");
        }

        if (tweak.Risk >= RiskLevel.Advanced)
            warnings.Add("Advanced system change. Explicit user approval is required.");

        if (tweak.RequiresRestart)
            warnings.Add("A restart may be required before the change is fully effective.");

        return new CompatibilityResult(
            reasons.Count == 0 && conflicts.Count == 0,
            reasons,
            warnings,
            conflicts);
    }
}
