using System.Text.Json;
using FarlaTweaks.Core.Models;

namespace FarlaTweaks.Core.Persistence;

public sealed class GameSessionStore
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Farla",
        "sessions.json");

    public async Task<IReadOnlyList<GameSession>> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_path))
            return Array.Empty<GameSession>();

        await using var stream = File.OpenRead(_path);
        return await JsonSerializer.DeserializeAsync<List<GameSession>>(stream, Options, cancellationToken)
               ?? new List<GameSession>();
    }

    public async Task AddAsync(GameSession session, CancellationToken cancellationToken = default)
    {
        var sessions = (await LoadAsync(cancellationToken)).ToList();
        sessions.Add(session);
        if (sessions.Count > 50)
            sessions = sessions.OrderByDescending(x => x.StartedAt).Take(50).Reverse().ToList();

        var directory = Path.GetDirectoryName(_path)
                        ?? throw new InvalidOperationException("Unable to determine Farla data directory.");
        Directory.CreateDirectory(directory);
        await using var stream = File.Create(_path);
        await JsonSerializer.SerializeAsync(stream, sessions, Options, cancellationToken);
    }
}
