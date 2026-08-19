using System.Text.Json;
using FarlaTweaks.Core.State;

namespace FarlaTweaks.Core.Persistence;

public sealed class SnapshotStore
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _directory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Farla",
        "snapshots");

    public async Task SaveAsync(StateSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, $"{snapshot.Id:N}.json");
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, snapshot, Options, cancellationToken);
    }

    public async Task<IReadOnlyList<StateSnapshot>> LoadAllAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_directory))
            return Array.Empty<StateSnapshot>();

        var snapshots = new List<StateSnapshot>();
        foreach (var path in Directory.EnumerateFiles(_directory, "*.json"))
        {
            await using var stream = File.OpenRead(path);
            var snapshot = await JsonSerializer.DeserializeAsync<StateSnapshot>(stream, Options, cancellationToken);
            if (snapshot is not null)
                snapshots.Add(snapshot);
        }

        return snapshots.OrderByDescending(x => x.CreatedAt).ToArray();
    }
}
