using FarlaTweaks.Core.Diagnostics;

namespace FarlaTweaks.Core.Monitoring;

public sealed class CopilotEngine
{
    public CopilotObservation Observe(PerformanceSample sample)
    {
        var now = sample.Timestamp;
        if (sample.MemoryPercent >= 92)
        {
            return new CopilotObservation(
                "attention",
                "Memory pressure detected",
                $"System memory is at {sample.MemoryPercent:0}%. Farla would investigate background applications before changing optimization settings.",
                now);
        }

        if (sample.CpuPercent >= 95 && (!sample.GpuPercent.HasValue || sample.GpuPercent.Value < 70))
        {
            return new CopilotObservation(
                "investigating",
                "CPU saturation detected",
                $"CPU usage is {sample.CpuPercent:0}% while GPU usage is not dominant. Farla would investigate CPU-heavy background work before recommending changes.",
                now);
        }

        if (sample.GpuPercent.HasValue && sample.GpuPercent.Value >= 97)
        {
            return new CopilotObservation(
                "normal",
                "GPU workload is high",
                $"GPU usage is {sample.GpuPercent.Value:0}%. Farla is observing a likely GPU-bound workload and will not claim that registry tweaks will improve it.",
                now);
        }

        return new CopilotObservation(
            "normal",
            "System is performing normally",
            "No high-confidence intervention is justified from the current telemetry.",
            now);
    }
}
