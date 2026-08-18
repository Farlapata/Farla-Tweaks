using Microsoft.Win32;
using FarlaTweaks.Core.Models;
using FarlaTweaks.Core.State;

namespace FarlaTweaks.Core.Execution;

public sealed class WindowsRegistryTweakExecutor : ITweakExecutor
{
    public Task<StateSnapshot> ApplyAsync(TweakDefinition tweak, CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Registry tweaks are supported only on Windows.");

        if (tweak.Risk == RiskLevel.Rejected)
            throw new InvalidOperationException("Rejected tweaks cannot be executed.");

        var snapshots = new List<RegistrySnapshot>();

        try
        {
            foreach (var change in tweak.RegistryChanges)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var baseKey = RegistryKey.OpenBaseKey(ParseHive(change.Root), RegistryView.Default);
                using var key = baseKey.CreateSubKey(change.KeyPath, writable: true)
                    ?? throw new InvalidOperationException($"Unable to open registry key '{change.KeyPath}'.");

                var existed = key.GetValueNames().Contains(change.ValueName, StringComparer.OrdinalIgnoreCase);
                var previous = existed
                    ? key.GetValue(change.ValueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames)
                    : null;
                var previousKind = existed ? key.GetValueKind(change.ValueName) : (RegistryValueKind?)null;

                snapshots.Add(new RegistrySnapshot(
                    change.Root,
                    change.KeyPath,
                    change.ValueName,
                    existed,
                    previousKind?.ToString(),
                    previous));

                key.SetValue(change.ValueName, ParseValue(change.ValueType, change.ValueData), ParseKind(change.ValueType));
            }
        }
        catch
        {
            TryRestore(snapshots);
            throw;
        }

        return Task.FromResult(new StateSnapshot(Guid.NewGuid(), DateTimeOffset.UtcNow, tweak.Name, snapshots));
    }

    public Task RevertAsync(StateSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Registry tweaks are supported only on Windows.");

        foreach (var item in snapshot.RegistryValues.Reverse())
        {
            cancellationToken.ThrowIfCancellationRequested();
            RestoreValue(item, snapshot.Id);
        }

        return Task.CompletedTask;
    }

    private static void TryRestore(IEnumerable<RegistrySnapshot> snapshots)
    {
        foreach (var item in snapshots.Reverse())
        {
            try
            {
                RestoreValue(item, Guid.Empty);
            }
            catch
            {
                // Preserve the original execution failure. The caller can surface the snapshot for manual recovery.
            }
        }
    }

    private static void RestoreValue(RegistrySnapshot item, Guid snapshotId)
    {
        using var baseKey = RegistryKey.OpenBaseKey(ParseHive(item.Root), RegistryView.Default);
        using var key = baseKey.OpenSubKey(item.KeyPath, writable: true)
            ?? baseKey.CreateSubKey(item.KeyPath, writable: true)
            ?? throw new InvalidOperationException($"Unable to open registry key '{item.KeyPath}'.");

        if (!item.Existed)
        {
            key.DeleteValue(item.ValueName, throwOnMissingValue: false);
            return;
        }

        if (item.ValueData is null)
            throw new InvalidOperationException($"Snapshot '{snapshotId}' does not contain a restorable value for '{item.ValueName}'.");

        var kind = Enum.TryParse<RegistryValueKind>(item.ValueType, ignoreCase: true, out var parsed)
            ? parsed
            : RegistryValueKind.String;
        key.SetValue(item.ValueName, item.ValueData, kind);
    }

    private static RegistryHive ParseHive(string root) => root.ToUpperInvariant() switch
    {
        "HKCU" or "HKEY_CURRENT_USER" => RegistryHive.CurrentUser,
        "HKLM" or "HKEY_LOCAL_MACHINE" => RegistryHive.LocalMachine,
        "HKCR" or "HKEY_CLASSES_ROOT" => RegistryHive.ClassesRoot,
        "HKU" or "HKEY_USERS" => RegistryHive.Users,
        "HKCC" or "HKEY_CURRENT_CONFIG" => RegistryHive.CurrentConfig,
        _ => throw new ArgumentException($"Unsupported registry root '{root}'.", nameof(root))
    };

    private static RegistryValueKind ParseKind(string type) => type.ToUpperInvariant() switch
    {
        "DWORD" => RegistryValueKind.DWord,
        "QWORD" => RegistryValueKind.QWord,
        "BINARY" => RegistryValueKind.Binary,
        "MULTI_SZ" => RegistryValueKind.MultiString,
        "EXPAND_SZ" => RegistryValueKind.ExpandString,
        _ => RegistryValueKind.String
    };

    private static object ParseValue(string type, string value)
    {
        return type.ToUpperInvariant() switch
        {
            "DWORD" => Convert.ToInt32(value, 16),
            "QWORD" => Convert.ToInt64(value, 16),
            "BINARY" => Convert.FromHexString(value.Replace(" ", string.Empty)),
            "MULTI_SZ" => value.Split("\\0", StringSplitOptions.None),
            _ => value
        };
    }
}
