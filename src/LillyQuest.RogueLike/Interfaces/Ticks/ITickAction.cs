using LillyQuest.RogueLike.Types.Tick;

namespace LillyQuest.RogueLike.Interfaces.Ticks;

/// <summary>
/// Represents a single action executed by the tick system.
/// </summary>
public interface ITickAction
{
    /// <summary>
    /// Unique identifier for the action instance.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Scheduling priority used to order actions.
    /// </summary>
    ActionPriority Priority { get; }

    /// <summary>
    /// Speed cost or delay for the action.
    /// </summary>
    int Speed { get; set; }

    /// <summary>
    /// Determines whether the action can be executed right now.
    /// </summary>
    bool CanBeExecuted();

    /// <summary>
    /// Executes the action and returns the result.
    /// </summary>
    ActionResult Execute();
}
