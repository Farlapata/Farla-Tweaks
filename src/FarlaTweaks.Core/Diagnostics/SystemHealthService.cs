using System.Diagnostics;

namespace FarlaTweaks.Core.Diagnostics;

public sealed class SystemHealthService
{
    public Task<IReadOnlyList<SystemHealthResult>> RunSafeChecksAsync(CancellationToken cancellationToken = default)
        => Task.Run(() =>
        {
            var results = new List<SystemHealthResult>
            {
                RunCommandCheck("DISM component store", "dism.exe", "/Online /Cleanup-Image /CheckHealth", cancellationToken),
                RunCommandCheck("Windows system files", "sfc.exe", "/verifyonly", cancellationToken)
            };
            return (IReadOnlyList<SystemHealthResult>)results;
        }, cancellationToken);

    private static SystemHealthResult RunCommandCheck(string check, string fileName, string arguments, CancellationToken cancellationToken)
    {
        var start = Stopwatch.GetTimestamp();
        try
        {
            var info = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(info);
            if (process is null)
                return new SystemHealthResult(check, false, $"Unable to start {fileName}.", Stopwatch.GetElapsedTime(start));

            while (!process.WaitForExit(250))
                cancellationToken.ThrowIfCancellationRequested();

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            var text = string.IsNullOrWhiteSpace(output) ? error : output;
            var summary = Collapse(text);

            return new SystemHealthResult(
                check,
                process.ExitCode == 0,
                string.IsNullOrWhiteSpace(summary) ? $"Exit code {process.ExitCode}." : summary,
                Stopwatch.GetElapsedTime(start));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new SystemHealthResult(check, false, ex.Message, Stopwatch.GetElapsedTime(start));
        }
    }

    private static string Collapse(string text)
    {
        var lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return lines.FirstOrDefault(line => line.Length > 12) ?? string.Empty;
    }
}
