using LillyQuest.RogueLike.Data.Scheduler;
using LillyQuest.RogueLike.Interfaces.Scheduler;

namespace LillyQuest.RogueLike.Interfaces.Services;

/// <summary>
/// Service interface for the turn-based scheduler.
/// </summary>
public interface ISchedulerService
{
    /// <summary>
    /// Current game tick.
    /// </summary>
    int CurrentTick { get; }

    /// <summary>
    /// Number of entities in the scheduler.
    /// </summary>
    int EntityCount { get; }

    /// <summary>
    /// Fired when an entity is about to act.
    /// </summary>
    event Action<ISchedulerEntity>? EntityActing;

    /// <summary>
    /// Fired after an action is executed.
    /// </summary>
    event Action<ActionExecutionRecord>? ActionExecuted;

    /// <summary>
    /// Fired when an entity is removed from the scheduler.
    /// </summary>
    event Action<ISchedulerEntity>? EntityRemoved;

    /// <summary>
    /// Adds an entity to the scheduler.
    /// </summary>
    void AddEntity(ISchedulerEntity entity);

    /// <summary>
    /// Clears all entities from the scheduler.
    /// </summary>
    void Clear();

    /// <summary>
    /// Clears the pending player action.
    /// </summary>
    void ClearPendingPlayerAction();

    /// <summary>
    /// Queues a player action to be executed when it's the player's turn.
    /// </summary>
    void EnqueuePlayerAction(ISchedulerAction action);

    /// <summary>
    /// Gets all entities ordered by current energy.
    /// </summary>
    IReadOnlyList<ISchedulerEntity> GetEntitiesByEnergy();

    /// <summary>
    /// Gets the player entity, if present.
    /// </summary>
    ISchedulerEntity? GetPlayer();

    /// <summary>
    /// Processes a single turn: finds the next entity that can act and executes their action.
    /// Call this from your game loop - it handles both AI and player turns.
    /// </summary>
    /// <returns>Result indicating what happened and if player input is needed.</returns>
    TurnResult ProcessNextTurn();

    /// <summary>
    /// Processes turns until player input is needed or scheduler is empty.
    /// </summary>
    /// <param name="maxIterations">Safety limit to prevent infinite loops.</param>
    TurnResult ProcessUntilPlayerInput(int maxIterations = 10000);

    /// <summary>
    /// Removes an entity from the scheduler.
    /// </summary>
    void RemoveEntity(ISchedulerEntity entity);

    /// <summary>
    /// Removes an entity by ID.
    /// </summary>
    void RemoveEntity(Guid entityId);

    /// <summary>
    /// Resets all entity energy and tick counter.
    /// </summary>
    void Reset();
}
