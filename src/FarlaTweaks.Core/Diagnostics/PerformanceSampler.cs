using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace FarlaTweaks.Core.Diagnostics;

public sealed class PerformanceSampler : IDisposable
{
    private ulong _lastIdle;
    private ulong _lastKernel;
    private ulong _lastUser;
    private bool _hasCpuBaseline;

    public PerformanceSample Sample()
    {
        var cpu = SampleCpuPercent();
        var memory = SampleMemoryPercent();
        var gpu = SampleNvidiaGpuPercent();

        return new PerformanceSample(DateTimeOffset.UtcNow, cpu, memory, gpu);
    }

    private double SampleCpuPercent()
    {
        if (!GetSystemTimes(out var idle, out var kernel, out var user))
            return 0;

        var idleTicks = FileTimeToUInt64(idle);
        var kernelTicks = FileTimeToUInt64(kernel);
        var userTicks = FileTimeToUInt64(user);

        if (!_hasCpuBaseline)
        {
            _lastIdle = idleTicks;
            _lastKernel = kernelTicks;
            _lastUser = userTicks;
            _hasCpuBaseline = true;
            return 0;
        }

        var idleDelta = idleTicks - _lastIdle;
        var totalDelta = (kernelTicks - _lastKernel) + (userTicks - _lastUser);
        _lastIdle = idleTicks;
        _lastKernel = kernelTicks;
        _lastUser = userTicks;

        if (totalDelta == 0)
            return 0;

        return Math.Clamp((1d - idleDelta / (double)totalDelta) * 100d, 0, 100);
    }

    private static double SampleMemoryPercent()
    {
        var status = new MemoryStatusEx { Length = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        if (!GlobalMemoryStatusEx(ref status) || status.TotalPhysicalMemory == 0)
            return 0;

        var used = status.TotalPhysicalMemory - status.AvailablePhysicalMemory;
        return Math.Clamp(used * 100d / status.TotalPhysicalMemory, 0, 100);
    }

    private static double? SampleNvidiaGpuPercent()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "nvidia-smi.exe",
                Arguments = "--query-gpu=utilization.gpu --format=csv,noheader,nounits",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                StandardOutputEncoding = Encoding.UTF8
            };

            using var process = Process.Start(startInfo);
            if (process is null)
                return null;

            if (!process.WaitForExit(800))
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                return null;
            }

            var output = process.StandardOutput.ReadToEnd().Trim();
            return double.TryParse(output.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(), out var value)
                ? Math.Clamp(value, 0, 100)
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static ulong FileTimeToUInt64(System.Runtime.InteropServices.ComTypes.FILETIME value)
        => ((ulong)value.dwHighDateTime << 32) | (ulong)(uint)value.dwLowDateTime;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemTimes(
        out System.Runtime.InteropServices.ComTypes.FILETIME idleTime,
        out System.Runtime.InteropServices.ComTypes.FILETIME kernelTime,
        out System.Runtime.InteropServices.ComTypes.FILETIME userTime);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx status);

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhysicalMemory;
        public ulong AvailablePhysicalMemory;
        public ulong TotalPageFile;
        public ulong AvailablePageFile;
        public ulong TotalVirtual;
        public ulong AvailableVirtual;
        public ulong AvailableExtendedVirtual;
    }

    public void Dispose()
    {
    }
}
