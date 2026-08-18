namespace FarlaTweaks.Core.State;

public sealed record StateSnapshot(
    Guid Id,
    DateTimeOffset CreatedAt,
    string Label,
    IReadOnlyList<RegistrySnapshot> RegistryValues);
