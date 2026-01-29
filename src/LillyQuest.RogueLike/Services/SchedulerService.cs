using LillyQuest.RogueLike.Data.Scheduler;
using LillyQuest.RogueLike.Interfaces.Scheduler;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Types.Scheduler;

namespace LillyQuest.RogueLike.Services;

/// <summary>
/// Service wrapper for TurnScheduler for DI registration.
/// </summary>
public sealed class SchedulerService : ISchedulerService
{
    private readonly TurnScheduler _turnScheduler = new();

    public int CurrentTick => _turnScheduler.CurrentTick;
    public int EntityCount => _turnScheduler.EntityCount;

    public event Action<ISchedulerEntity>? EntityActing
    {
        add => _turnScheduler.EntityActing += value;
        remove => _turnScheduler.EntityActing -= value;
    }

    public event Action<ActionExecutionRecord>? ActionExecuted
    {
        add => _turnScheduler.ActionExecuted += value;
        remove => _turnScheduler.ActionExecuted -= value;
    }

    public event Action<ISchedulerEntity>? EntityRemoved
    {
        add => _turnScheduler.EntityRemoved += value;
        remove => _turnScheduler.EntityRemoved -= value;
    }

    public void AddEntity(ISchedulerEntity entity)
        => _turnScheduler.AddEntity(entity);

    public void Clear()
        => _turnScheduler.Clear();

    public TurnResult ProcessNextTurn()
        => _turnScheduler.ProcessNextTurn();

    public void ClearPendingPlayerAction()
        => _turnScheduler.ClearPendingPlayerAction();

    public void EnqueuePlayerAction(ISchedulerAction action)
        => _turnScheduler.EnqueuePlayerAction(action);

    public IReadOnlyList<ISchedulerEntity> GetEntitiesByEnergy()
        => _turnScheduler.GetEntitiesByEnergy();

    public ISchedulerEntity? GetPlayer()
        => _turnScheduler.GetPlayer();

    public TurnResult ProcessUntilPlayerInput(int maxIterations = 10000)
        => _turnScheduler.ProcessUntilPlayerInput(maxIterations);

    public void RemoveEntity(ISchedulerEntity entity)
        => _turnScheduler.RemoveEntity(entity);

    public void RemoveEntity(Guid entityId)
        => _turnScheduler.RemoveEntity(entityId);

    public void Reset()
        => _turnScheduler.Reset();
}
