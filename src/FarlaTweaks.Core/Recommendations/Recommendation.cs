using FarlaTweaks.Core.Models;

namespace FarlaTweaks.Core.Recommendations;

public sealed record Recommendation(
    string TweakId,
    string Title,
    string Reason,
    RiskLevel Risk,
    bool RequiresRestart);
