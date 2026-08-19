namespace FarlaTweaks.Core.Diagnostics;

public sealed class FarlaLogger
{
    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Farla",
        "logs",
        "farla.log");

    public void Info(string message) => Write("INFO", message);

    public void Warn(string message) => Write("WARN", message);

    public void Error(string message, Exception? exception = null)
    {
        var detail = exception is null ? message : $"{message} | {exception.GetType().Name}: {exception.Message}";
        Write("ERROR", detail);
    }

    private void Write(string level, string message)
    {
        try
        {
            var directory = Path.GetDirectoryName(_path);
            if (string.IsNullOrWhiteSpace(directory))
                return;

            Directory.CreateDirectory(directory);
            File.AppendAllText(_path, $"[{DateTimeOffset.Now:O}] [{level}] {message}{Environment.NewLine}");
        }
        catch
        {
            // Logging must never break the application.
        }
    }
}
