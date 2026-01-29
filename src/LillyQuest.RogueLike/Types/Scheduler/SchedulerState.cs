namespace LillyQuest.RogueLike.Types.Scheduler;

/// <summary>
/// The current state of the scheduler.
/// </summary>
public enum SchedulerState
{
    /// <summary>
    /// Processing entities, no player input needed yet.
    /// </summary>
    Processing,

    /// <summary>
    /// Waiting for player to provide an action.
    /// Call EnqueuePlayerAction() then ProcessTurn() again.
    /// </summary>
    WaitingForPlayerInput,

    /// <summary>
    /// Player's action was blocked (CanExecute returned false).
    /// Player should choose a different action.
    /// </summary>
    PlayerActionBlocked,

    /// <summary>
    /// Player cancelled their action.
    /// Player should choose a different action.
    /// </summary>
    PlayerActionCancelled,

    /// <summary>
    /// No active entities in the scheduler.
    /// </summary>
    Empty
}
