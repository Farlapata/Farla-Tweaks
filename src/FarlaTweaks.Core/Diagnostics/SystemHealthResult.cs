namespace FarlaTweaks.Core.Diagnostics;

public sealed record SystemHealthResult(
    string Check,
    bool Success,
    string Summary,
    TimeSpan Duration);
