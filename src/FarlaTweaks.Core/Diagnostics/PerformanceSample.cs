namespace FarlaTweaks.Core.Diagnostics;

public sealed record PerformanceSample(
    DateTimeOffset Timestamp,
    double CpuPercent,
    double MemoryPercent,
    double? GpuPercent);
