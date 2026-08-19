namespace FarlaTweaks.Core.Licensing;

public interface ILicenseService
{
    Task<LicenseState> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<LicenseState> ActivateAsync(string licenseKey, CancellationToken cancellationToken = default);
}
