using System.Management;
using FarlaTweaks.Core.Models;

namespace FarlaTweaks.Core.System;

public sealed class WindowsSystemProfileScanner
{
    public SystemProfile Scan()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Farla Tweaks currently targets Windows.");

        var cpu = QueryFirst("Win32_Processor", "Name") ?? Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "Unknown CPU";
        var gpu = QueryFirst("Win32_VideoController", "Name") ?? "Unknown GPU";
        var totalRamGb = Math.Max(1, (int)Math.Round(GetTotalPhysicalMemoryBytes() / 1024d / 1024d / 1024d));

        return new SystemProfile
        {
            OsFamily = "Windows",
            OsVersion = Environment.OSVersion.VersionString,
            Architecture = Environment.Is64BitOperatingSystem ? "x64" : "x86",
            Cpu = cpu,
            Gpu = gpu,
            RamGb = totalRamGb,
            PrimaryDisplay = QueryFirst("Win32_DesktopMonitor", "Name") ?? "Unknown display",
            RefreshRateHz = QueryInt("Win32_VideoController", "CurrentRefreshRate"),
            Capabilities = Array.Empty<string>(),
            DetectedApplications = Array.Empty<string>()
        };
    }

    private static string? QueryFirst(string className, string property)
    {
        using var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {className}");
        using var results = searcher.Get();
        foreach (ManagementObject item in results)
        {
            var value = item[property]?.ToString();
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }
        return null;
    }

    private static int QueryInt(string className, string property)
    {
        using var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {className}");
        using var results = searcher.Get();
        foreach (ManagementObject item in results)
        {
            if (item[property] is int value && value > 0)
                return value;
        }
        return 0;
    }

    private static ulong GetTotalPhysicalMemoryBytes()
    {
        using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
        using var results = searcher.Get();
        foreach (ManagementObject item in results)
        {
            if (item["TotalPhysicalMemory"] is ulong value)
                return value;
            if (ulong.TryParse(item["TotalPhysicalMemory"]?.ToString(), out value))
                return value;
        }
        return 0;
    }
}
