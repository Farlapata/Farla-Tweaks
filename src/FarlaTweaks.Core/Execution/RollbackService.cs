using FarlaTweaks.Core.Persistence;
using FarlaTweaks.Core.State;

namespace FarlaTweaks.Core.Execution;

public sealed class RollbackService
{
    private readonly SnapshotStore _snapshotStore;
    private readonly ITweakExecutor _executor;

    public RollbackService(SnapshotStore snapshotStore, ITweakExecutor executor)
    {
        _snapshotStore = snapshotStore;
        _executor = executor;
    }

    public async Task<int> RevertAllAsync(CancellationToken cancellationToken = default)
    {
        var snapshots = await _snapshotStore.LoadAllAsync(cancellationToken);
        var reverted = 0;
        foreach (var snapshot in snapshots.OrderByDescending(x => x.CreatedAt))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _executor.RevertAsync(snapshot, cancellationToken);
            reverted++;
        }

        return reverted;
    }
}
