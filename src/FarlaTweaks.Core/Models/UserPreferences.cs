namespace FarlaTweaks.Core.Models;

public sealed record UserPreferences
{
    public bool OnboardingCompleted { get; init; }
    public string PrimaryGame { get; init; } = "Fortnite";
    public IReadOnlyList<string> Goals { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}
