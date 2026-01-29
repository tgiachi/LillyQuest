namespace LillyQuest.RogueLike.Types.Scheduler;

/// <summary>
/// Result of executing a scheduler action.
/// </summary>
public enum ActionResult
{
    /// <summary>
    /// Action completed successfully. Energy cost is deducted.
    /// </summary>
    Success,

    /// <summary>
    /// Action failed validation (CanExecute returned false).
    /// Entity keeps their turn to try another action.
    /// </summary>
    Blocked,

    /// <summary>
    /// Action failed during execution.
    /// Energy cost is still deducted (you tried and failed).
    /// </summary>
    Failed,

    /// <summary>
    /// Action was cancelled (e.g., player pressed escape).
    /// Entity keeps their turn.
    /// </summary>
    Cancelled
}
