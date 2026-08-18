namespace FarlaTweaks.Core.State;

public sealed record RegistrySnapshot(
    string Root,
    string KeyPath,
    string ValueName,
    bool Existed,
    string? ValueType,
    object? ValueData);
