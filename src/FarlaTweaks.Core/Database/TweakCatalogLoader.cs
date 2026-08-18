using System.Text.Json;
using FarlaTweaks.Core.Models;

namespace FarlaTweaks.Core.Database;

public sealed class TweakCatalogLoader
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public async Task<IReadOnlyList<TweakDefinition>> LoadAsync(CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "database", "starter", "tweaks.json");
        if (!File.Exists(path))
            return Array.Empty<TweakDefinition>();

        await using var stream = File.OpenRead(path);
        var result = await JsonSerializer.DeserializeAsync<List<TweakDefinition>>(stream, Options, cancellationToken);
        return result is { Count: > 0 } ? result : Array.Empty<TweakDefinition>();
    }
}
