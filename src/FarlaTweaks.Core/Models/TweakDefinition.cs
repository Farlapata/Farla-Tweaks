namespace FarlaTweaks.Core.Models;

public enum RiskLevel
{
    Safe,
    Moderate,
    Advanced,
    Rejected
}

public sealed record RegistryChange(
    string Root,
    string KeyPath,
    string ValueName,
    string ValueType,
    string ValueData);

public sealed record TweakDefinition
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; }
    public string Description { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public RiskLevel Risk { get; init; } = RiskLevel.Safe;
    public bool RequiresRestart { get; init; }
    public string RequiredOsFamily { get; init; } = "Windows";
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Conflicts { get; init; } = Array.Empty<string>();
    public IReadOnlyList<RegistryChange> RegistryChanges { get; init; } = Array.Empty<RegistryChange>();
}
