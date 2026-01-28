using LillyQuest.RogueLike.Interfaces.Scheduler;
using LillyQuest.RogueLike.Types.Scheduler;
using Serilog;

namespace LillyQuest.RogueLike.Data.Scheduler;

/// <summary>
/// Turn-based scheduler for roguelike games.
/// Entities accumulate energy based on speed and act when they reach the threshold.
/// Properly interleaves player and AI turns based on relative speeds.
/// </summary>
public sealed class Scheduler
{
    /// <summary>
    /// Energy threshold required to take an action.
    /// </summary>
    public const int EnergyThreshold = 100;

    /// <summary>
    /// Default energy cost for a standard action.
    /// </summary>
    public const int StandardActionCost = 100;

    private readonly ILogger _logger = Log.ForContext<Scheduler>();
    private readonly List<ISchedulerEntity> _entities = [];
    private readonly List<ActionExecutionRecord> _currentCycleActions = [];
    private readonly List<ISchedulerEntity> _removedThisCycle = [];

    private ISchedulerAction? _pendingPlayerAction;

    /// <summary>
    /// Current game tick. Increments each time energy is distributed.
    /// </summary>
    public int CurrentTick { get; private set; }

    /// <summary>
    /// Number of entities in the scheduler.
    /// </summary>
    public int EntityCount => _entities.Count;

    /// <summary>
    /// Fired when an entity is about to act.
    /// </summary>
    public event Action<ISchedulerEntity>? EntityActing;

    /// <summary>
    /// Fired after an action is executed.
    /// </summary>
    public event Action<ActionExecutionRecord>? ActionExecuted;

    /// <summary>
    /// Fired when an entity is removed from the scheduler.
    /// </summary>
    public event Action<ISchedulerEntity>? EntityRemoved;

    /// <summary>
    /// Adds an entity to the scheduler.
    /// </summary>
    public void AddEntity(ISchedulerEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (_entities.Any(e => e.Id == entity.Id))
        {
            _logger.Warning("Entity {EntityId} already in scheduler", entity.Id);

            return;
        }

        _entities.Add(entity);
        _logger.Debug(
            "Added entity {EntityId} (Speed={Speed}, IsPlayer={IsPlayer})",
            entity.Id,
            entity.Speed,
            entity.IsPlayer
        );
    }

    /// <summary>
    /// Clears all entities from the scheduler.
    /// </summary>
    public void Clear()
    {
        _entities.Clear();
        _pendingPlayerAction = null;
        CurrentTick = 0;
    }

    /// <summary>
    /// Clears the pending player action (e.g., player pressed escape).
    /// </summary>
    public void ClearPendingPlayerAction()
    {
        _pendingPlayerAction = null;
    }

    /// <summary>
    /// Queues a player action to be executed when it's the player's turn.
    /// </summary>
    public void EnqueuePlayerAction(ISchedulerAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (!action.Actor.IsPlayer)
        {
            throw new ArgumentException("EnqueuePlayerAction requires a player action", nameof(action));
        }

        _pendingPlayerAction = action;
        _logger.Debug(
            "Queued player action {ActionId} (Cost={Cost})",
            action.Id,
            action.EnergyCost
        );
    }

    /// <summary>
    /// Processes a single turn: finds the next entity that can act and executes their action.
    /// Call this from your game loop - it handles both AI and player turns.
    /// </summary>
    /// <returns>Result indicating what happened and if player input is needed.</returns>
    public TurnResult ProcessNextTurn()
    {
        _currentCycleActions.Clear();
        _removedThisCycle.Clear();

        // Remove inactive entities
        CleanupInactiveEntities();

        if (_entities.Count == 0)
        {
            return CreateResult(SchedulerState.Empty);
        }

        // Distribute energy until someone can act
        var readyEntity = GetNextReadyEntity();

        while (readyEntity == null)
        {
            var hasPlayer = _entities.Any(e => e.IsPlayer);

            if (!hasPlayer)
            {
                // No player and no one ready - nothing to do
                return CreateResult(SchedulerState.Empty);
            }

            DistributeEnergy();
            readyEntity = GetNextReadyEntity();
        }

        // Player's turn
        if (readyEntity.IsPlayer)
        {
            if (_pendingPlayerAction == null)
            {
                return CreateResult(SchedulerState.WaitingForPlayerInput, readyEntity);
            }

            var playerResult = ExecuteAction(readyEntity, _pendingPlayerAction);
            _pendingPlayerAction = null;

            if (playerResult.Result == ActionResult.Blocked)
            {
                return CreateResult(SchedulerState.PlayerActionBlocked, readyEntity);
            }

            if (playerResult.Result == ActionResult.Cancelled)
            {
                return CreateResult(SchedulerState.PlayerActionCancelled, readyEntity);
            }

            return CreateResult(SchedulerState.Processing, readyEntity);
        }

        // AI entity's turn
        var aiAction = readyEntity.GetNextAction();

        if (aiAction == null)
        {
            readyEntity.Energy -= StandardActionCost;
            _logger.Debug("Entity {EntityId} skipped turn (no action)", readyEntity.Id);
        }
        else
        {
            ExecuteAction(readyEntity, aiAction);
        }

        return CreateResult(SchedulerState.Processing, readyEntity);
    }

    /// <summary>
    /// Gets all entities ordered by current energy (highest first).
    /// </summary>
    public IReadOnlyList<ISchedulerEntity> GetEntitiesByEnergy()
    {
        return _entities
               .OrderByDescending(e => e.Energy)
               .ThenByDescending(e => e.Speed)
               .ThenBy(e => e.Id)
               .ToList();
    }

    /// <summary>
    /// Gets the player entity, if present.
    /// </summary>
    public ISchedulerEntity? GetPlayer()
    {
        return _entities.FirstOrDefault(e => e.IsPlayer);
    }

    /// <summary>
    /// Processes turns until player input is needed or scheduler is empty.
    /// Call this after player input to continue processing.
    /// </summary>
    /// <param name="maxIterations">Safety limit to prevent infinite loops. Default 10000.</param>
    public TurnResult ProcessUntilPlayerInput(int maxIterations = 10000)
    {
        _currentCycleActions.Clear();
        _removedThisCycle.Clear();

        if (_entities.Count == 0)
        {
            return CreateResult(SchedulerState.Empty);
        }

        var iterations = 0;

        // Main loop: keep processing until we need player input
        while (iterations++ < maxIterations)
        {
            // Remove inactive entities
            CleanupInactiveEntities();

            if (_entities.Count == 0)
            {
                return CreateResult(SchedulerState.Empty);
            }

            // Check if there's a player - if not and no ready entities, return empty
            var hasPlayer = _entities.Any(e => e.IsPlayer);

            // Find entity with highest energy that can act
            var readyEntity = GetNextReadyEntity();

            if (readyEntity == null)
            {
                // No one can act yet
                if (!hasPlayer)
                {
                    // No player and no one ready - would loop forever distributing energy
                    return CreateResult(SchedulerState.Empty);
                }

                // Distribute energy and continue
                DistributeEnergy();

                continue;
            }

            // Player's turn - check if we have an action queued
            if (readyEntity.IsPlayer)
            {
                if (_pendingPlayerAction == null)
                {
                    // Wait for player input
                    return CreateResult(SchedulerState.WaitingForPlayerInput, readyEntity);
                }

                // Execute player action
                var playerResult = ExecuteAction(readyEntity, _pendingPlayerAction);
                _pendingPlayerAction = null;

                if (playerResult.Result == ActionResult.Blocked)
                {
                    return CreateResult(SchedulerState.PlayerActionBlocked, readyEntity);
                }

                if (playerResult.Result == ActionResult.Cancelled)
                {
                    return CreateResult(SchedulerState.PlayerActionCancelled, readyEntity);
                }

                // Player acted successfully - now process other entities until player needs to act again
                continue;
            }

            // AI entity's turn
            var aiAction = readyEntity.GetNextAction();

            if (aiAction == null)
            {
                // AI has no action (stunned, confused, etc) - skip and deduct standard cost
                readyEntity.Energy -= StandardActionCost;
                _logger.Debug("Entity {EntityId} skipped turn (no action)", readyEntity.Id);

                continue;
            }

            ExecuteAction(readyEntity, aiAction);
        }

        _logger.Warning("ProcessUntilPlayerInput hit max iterations ({Max})", maxIterations);

        return CreateResult(SchedulerState.Processing);
    }

    /// <summary>
    /// Removes an entity from the scheduler.
    /// </summary>
    public void RemoveEntity(ISchedulerEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (_entities.Remove(entity))
        {
            _logger.Debug("Removed entity {EntityId}", entity.Id);
            EntityRemoved?.Invoke(entity);
        }
    }

    /// <summary>
    /// Removes an entity by ID.
    /// </summary>
    public void RemoveEntity(Guid entityId)
    {
        var entity = _entities.FirstOrDefault(e => e.Id == entityId);

        if (entity != null)
        {
            RemoveEntity(entity);
        }
    }

    /// <summary>
    /// Resets all entity energy to 0 and tick counter.
    /// </summary>
    public void Reset()
    {
        foreach (var entity in _entities)
        {
            entity.Energy = 0;
        }
        CurrentTick = 0;
        _pendingPlayerAction = null;
        _currentCycleActions.Clear();
        _removedThisCycle.Clear();
    }

    private void CleanupInactiveEntities()
    {
        var toRemove = _entities.Where(e => !e.IsActive).ToList();

        foreach (var entity in toRemove)
        {
            _entities.Remove(entity);
            _removedThisCycle.Add(entity);
            EntityRemoved?.Invoke(entity);
            _logger.Debug("Removed inactive entity {EntityId}", entity.Id);
        }
    }

    private TurnResult CreateResult(SchedulerState state, ISchedulerEntity? activeEntity = null)
        => new()
        {
            State = state,
            ActiveEntity = activeEntity,
            ExecutedActions = _currentCycleActions.ToList(),
            CurrentTick = CurrentTick,
            RemovedEntities = _removedThisCycle.ToList()
        };

    private void DistributeEnergy()
    {
        CurrentTick++;

        foreach (var entity in _entities)
        {
            if (!entity.IsActive)
            {
                continue;
            }

            entity.Energy += entity.Speed;
        }

        _logger.Verbose(
            "Tick {Tick}: Distributed energy to {Count} entities",
            CurrentTick,
            _entities.Count
        );
    }

    private ActionExecutionRecord ExecuteAction(ISchedulerEntity entity, ISchedulerAction action)
    {
        EntityActing?.Invoke(entity);

        _logger.Debug(
            "Entity {EntityId} executing {ActionType} (Cost={Cost})",
            entity.Id,
            action.GetType().Name,
            action.EnergyCost
        );

        ActionResult result;
        var energyCost = 0;

        try
        {
            if (!action.CanExecute())
            {
                result = ActionResult.Blocked;
                _logger.Debug("Action blocked for entity {EntityId}", entity.Id);
            }
            else
            {
                result = action.Execute();
                energyCost = action.EnergyCost;

                // Deduct energy on success or failure (you tried)
                if (result is ActionResult.Success or ActionResult.Failed)
                {
                    entity.Energy -= energyCost;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Action execution failed for entity {EntityId}", entity.Id);
            result = ActionResult.Failed;
            energyCost = action.EnergyCost;
            entity.Energy -= energyCost;
        }

        var record = new ActionExecutionRecord
        {
            Action = action,
            Actor = entity,
            Result = result,
            EnergyCostPaid = energyCost,
            ActorEnergyAfter = entity.Energy
        };

        _currentCycleActions.Add(record);
        ActionExecuted?.Invoke(record);

        _logger.Debug(
            "Entity {EntityId} action result: {Result}, Energy now: {Energy}",
            entity.Id,
            result,
            entity.Energy
        );

        return record;
    }

    private ISchedulerEntity? GetNextReadyEntity()
    {
        // Get entity with highest energy >= threshold
        // Ties broken by speed (faster acts first), then by ID (deterministic)
        return _entities
               .Where(e => e.Energy >= EnergyThreshold && e.IsActive)
               .OrderByDescending(e => e.Energy)
               .ThenByDescending(e => e.Speed)
               .ThenBy(e => e.Id)
               .FirstOrDefault();
    }
}
