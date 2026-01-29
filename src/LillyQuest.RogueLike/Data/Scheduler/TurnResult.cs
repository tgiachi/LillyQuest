using LillyQuest.RogueLike.Interfaces.Scheduler;
using LillyQuest.RogueLike.Types.Scheduler;

namespace LillyQuest.RogueLike.Data.Scheduler;

/// <summary>
/// Result of processing turns in the scheduler.
/// </summary>
public sealed class TurnResult
{
    /// <summary>
    /// Current state of the scheduler.
    /// </summary>
    public required SchedulerState State { get; init; }

    /// <summary>
    /// The entity that needs to act (when WaitingForPlayerInput).
    /// </summary>
    public ISchedulerEntity? ActiveEntity { get; init; }

    /// <summary>
    /// Actions that were executed during this processing cycle.
    /// </summary>
    public IReadOnlyList<ActionExecutionRecord> ExecutedActions { get; init; } = [];

    /// <summary>
    /// Current game tick (increments when all entities have had a chance to gain energy).
    /// </summary>
    public int CurrentTick { get; init; }

    /// <summary>
    /// Entities that were removed during processing (died, etc).
    /// </summary>
    public IReadOnlyList<ISchedulerEntity> RemovedEntities { get; init; } = [];
}
