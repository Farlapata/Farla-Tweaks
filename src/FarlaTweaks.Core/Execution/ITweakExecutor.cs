using FarlaTweaks.Core.Models;
using FarlaTweaks.Core.State;

namespace FarlaTweaks.Core.Execution;

public interface ITweakExecutor
{
    Task<StateSnapshot> ApplyAsync(TweakDefinition tweak, CancellationToken cancellationToken = default);
    Task RevertAsync(StateSnapshot snapshot, CancellationToken cancellationToken = default);
}
