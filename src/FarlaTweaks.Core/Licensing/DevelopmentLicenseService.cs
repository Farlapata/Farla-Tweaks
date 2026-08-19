namespace FarlaTweaks.Core.Licensing;

public sealed class DevelopmentLicenseService : ILicenseService
{
    public Task<LicenseState> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new LicenseState(
            IsActivated: true,
            Tier: LicenseTier.Alpha,
            MaskedLicenseKey: null,
            ExpiresAt: null,
            DeviceBinding: Environment.MachineName));
    }

    public Task<LicenseState> ActivateAsync(string licenseKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            throw new ArgumentException("A license key is required.", nameof(licenseKey));

        throw new InvalidOperationException("Commercial activation is disabled in development builds. The stable release will use the remote licensing service.");
    }
}
