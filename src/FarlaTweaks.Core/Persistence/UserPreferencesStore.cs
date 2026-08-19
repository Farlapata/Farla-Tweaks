using System.Text.Json;
using FarlaTweaks.Core.Models;

namespace FarlaTweaks.Core.Persistence;

public sealed class UserPreferencesStore
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Farla",
        "preferences.json");

    public async Task<UserPreferences?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_path))
            return null;

        await using var stream = File.OpenRead(_path);
        return await JsonSerializer.DeserializeAsync<UserPreferences>(stream, Options, cancellationToken);
    }

    public async Task SaveAsync(UserPreferences preferences, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(_path)
            ?? throw new InvalidOperationException("Unable to determine Farla preferences directory.");

        Directory.CreateDirectory(directory);
        await using var stream = File.Create(_path);
        await JsonSerializer.SerializeAsync(stream, preferences, Options, cancellationToken);
    }
}
