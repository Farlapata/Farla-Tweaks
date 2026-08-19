namespace FarlaTweaks.Core.Licensing;

public enum LicenseTier
{
    Alpha,
    Free,
    Pro
}

public sealed record LicenseState(
    bool IsActivated,
    LicenseTier Tier,
    string? MaskedLicenseKey,
    DateTimeOffset? ExpiresAt,
    string DeviceBinding);
