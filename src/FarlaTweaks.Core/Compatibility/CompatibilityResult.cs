namespace FarlaTweaks.Core.Compatibility;

public sealed record CompatibilityResult(
    bool IsCompatible,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Conflicts);
