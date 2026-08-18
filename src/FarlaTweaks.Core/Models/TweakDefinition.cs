namespace FarlaTweaks.Core.Models;

public sealed record TweakDefinition
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Category { get; init; }
    public required TweakRisk Risk { get; init; }
    public required RestartRequirement Restart { get; init; }
    public IReadOnlyList<string> Dependencies { get; init; } = [];
    public IReadOnlyList<string> Conflicts { get; init; } = [];
    public IReadOnlyList<TweakChange> Changes { get; init; } = [];
}

public enum TweakRisk
{
    Low,
    Moderate,
    High,
    Experimental
}

public enum RestartRequirement
{
    None,
    Explorer,
    Application,
    SignOut,
    Reboot
}

public sealed record TweakChange
{
    public required string Type { get; init; }
    public required string Target { get; init; }
    public required string ValueName { get; init; }
    public string? Before { get; init; }
    public string? After { get; init; }
}
