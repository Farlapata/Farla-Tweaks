using FarlaTweaks.Core.Compatibility;
using FarlaTweaks.Core.Models;

namespace FarlaTweaks.Core.Recommendations;

public sealed class RecommendationEngine
{
    private readonly CompatibilityEngine _compatibilityEngine = new();

    public IReadOnlyList<Recommendation> Build(SystemProfile profile, IEnumerable<TweakDefinition> tweaks, IEnumerable<string> selectedDependencies)
    {
        var selected = selectedDependencies.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var recommendations = new List<Recommendation>();

        foreach (var tweak in tweaks)
        {
            if (tweak.Risk == RiskLevel.Rejected)
                continue;

            var result = _compatibilityEngine.Evaluate(tweak, profile, selected);
            if (!result.IsCompatible)
                continue;

            if (tweak.Tags.Contains("capture", StringComparer.OrdinalIgnoreCase) &&
                selected.Contains("game-bar-required"))
                continue;

            recommendations.Add(new Recommendation(
                tweak.Id,
                tweak.Name,
                BuildReason(tweak, selected),
                tweak.Risk,
                tweak.RequiresRestart));
        }

        return recommendations
            .OrderBy(r => r.Risk)
            .ThenBy(r => r.Title, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string BuildReason(TweakDefinition tweak, IReadOnlySet<string> selected)
    {
        if (tweak.Tags.Contains("capture", StringComparer.OrdinalIgnoreCase) &&
            selected.Contains("game-bar-unused"))
            return "You indicated that Game Bar capture is not needed, so Farla can consider reducing related background activity.";

        if (tweak.Tags.Contains("startup", StringComparer.OrdinalIgnoreCase))
            return "Farla found a safe review opportunity instead of automatically disabling unknown startup software.";

        return tweak.Purpose;
    }
}
