using LillyQuest.RogueLike.Types.Scheduler;

namespace LillyQuest.RogueLike.Interfaces.Scheduler;

/// <summary>
/// An action in the turn-based scheduler system.
/// Each action has an energy cost that determines how long until the entity can act again.
/// </summary>
public interface ISchedulerAction
{
    /// <summary>
    /// Unique identifier for this action instance.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Energy cost of this action. Standard action = 100.
    /// Lower cost = can act again sooner.
    /// Examples: Move=100, Attack=100, QuickAttack=75, HeavyAttack=150, Wait=100
    /// </summary>
    int EnergyCost { get; }

    /// <summary>
    /// The entity performing this action.
    /// </summary>
    ISchedulerEntity Actor { get; }

    /// <summary>
    /// Validates whether this action can be executed.
    /// Called right before execution to check preconditions.
    /// </summary>
    bool CanExecute();

    /// <summary>
    /// Executes the action and returns the result.
    /// </summary>
    ActionResult Execute();
}
