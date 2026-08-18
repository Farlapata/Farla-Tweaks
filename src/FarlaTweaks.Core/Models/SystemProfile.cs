namespace FarlaTweaks.Core.Models;

public sealed record SystemProfile
{
    public string OsFamily { get; init; } = "Windows";
    public string OsVersion { get; init; } = string.Empty;
    public string Architecture { get; init; } = string.Empty;
    public string Cpu { get; init; } = string.Empty;
    public string Gpu { get; init; } = string.Empty;
    public int RamGb { get; init; }
    public string PrimaryDisplay { get; init; } = string.Empty;
    public int RefreshRateHz { get; init; }
    public IReadOnlyList<string> Capabilities { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> DetectedApplications { get; init; } = Array.Empty<string>();
}
