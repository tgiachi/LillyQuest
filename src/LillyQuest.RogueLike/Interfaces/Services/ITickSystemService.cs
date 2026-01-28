
using LillyQuest.RogueLike.Interfaces.Ticks;

namespace LillyQuest.RogueLike.Interfaces.Services;

/// <summary>
/// Service for managing tick-based updates and queued tick actions.
/// </summary>
public interface ITickSystemService
{
    /// <summary>
    /// Delegate invoked on tick events with the current tick count.
    /// </summary>
    delegate void TickDelegate(int tickCount);

    /// <summary>
    /// Current tick counter.
    /// </summary>
    int TickCount { get; }

    /// <summary>
    /// Raised when a tick completes.
    /// </summary>
    event TickDelegate Tick;

    /// <summary>
    /// Raised before executing a tick.
    /// </summary>
    event TickDelegate TickStarted;

    /// <summary>
    /// Raised after executing a tick.
    /// </summary>
    event TickDelegate TickEnded;

    /// <summary>
    /// Executes a single tick.
    /// </summary>
    void ExecuteTick();

    /// <summary>
    /// Enqueues multiple tick actions to be executed on future ticks.
    /// </summary>
    void EnqueueActions(IEnumerable<ITickAction> actions);

    /// <summary>
    /// Enqueues a single tick action to be executed on a future tick.
    /// </summary>
    void EnqueueAction(ITickAction action);

    /// <summary>
    /// Clears any continuing tick actions.
    /// </summary>
    void ClearContinuingActions();
}
