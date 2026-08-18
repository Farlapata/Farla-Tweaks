using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using FarlaTweaks.Core.Models;

namespace FarlaTweaks.Core.Diagnostics;

public sealed class SystemProfileCollector
{
    public SystemProfile Collect()
    {
        return new SystemProfile
        {
            OsFamily = "Windows",
            OsVersion = GetWindowsVersion(),
            Architecture = RuntimeInformation.OSArchitecture.ToString(),
            Cpu = FirstManagementValue("Win32_Processor", "Name"),
            Gpu = FirstManagementValue("Win32_VideoController", "Name"),
            RamGb = GetPhysicalMemoryGb(),
            PrimaryDisplay = FirstManagementValue("Win32_DesktopMonitor", "Name"),
            RefreshRateHz = GetPrimaryRefreshRate(),
            Capabilities = DetectCapabilities(),
            DetectedApplications = Array.Empty<string>()
        };
    }

    private static string GetWindowsVersion()
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
        if (key is null)
            return Environment.OSVersion.Version.ToString();

        var productName = key.GetValue("ProductName")?.ToString() ?? "Windows";
        var displayVersion = key.GetValue("DisplayVersion")?.ToString();
        return string.IsNullOrWhiteSpace(displayVersion) ? productName : $"{productName} {displayVersion}";
    }

    private static string FirstManagementValue(string className, string propertyName)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {className}");
            foreach (ManagementObject item in searcher.Get())
            {
                var value = item[propertyName]?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }
        catch
        {
            // Hardware discovery is best-effort and must never prevent the app from starting.
        }

        return "Unknown";
    }

    private static int GetPhysicalMemoryGb()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            foreach (ManagementObject item in searcher.Get())
            {
                if (ulong.TryParse(item["TotalPhysicalMemory"]?.ToString(), out var bytes) && bytes > 0)
                    return Math.Max(1, (int)Math.Round(bytes / 1024d / 1024d / 1024d));
            }
        }
        catch
        {
            // Best-effort only.
        }

        return 0;
    }

    private static int GetPrimaryRefreshRate()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT CurrentRefreshRate FROM Win32_VideoController");
            foreach (ManagementObject item in searcher.Get())
            {
                if (int.TryParse(item["CurrentRefreshRate"]?.ToString(), out var refresh) && refresh > 0)
                    return refresh;
            }
        }
        catch
        {
            // Best-effort only.
        }

        return 0;
    }

    private static IReadOnlyList<string> DetectCapabilities()
    {
        var capabilities = new List<string>
        {
            "windows"
        };

        if (Environment.Is64BitOperatingSystem)
            capabilities.Add("windows.x64");

        return capabilities;
    }
}
