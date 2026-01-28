using LillyQuest.RogueLike.Data.Scheduler;
using LillyQuest.RogueLike.Interfaces.Scheduler;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Types.Scheduler;

namespace LillyQuest.RogueLike.Services;

/// <summary>
/// Service wrapper for Scheduler for DI registration.
/// </summary>
public sealed class SchedulerService : ISchedulerService
{
    private readonly Scheduler _scheduler = new();

    public int CurrentTick => _scheduler.CurrentTick;
    public int EntityCount => _scheduler.EntityCount;

    public event Action<ISchedulerEntity>? EntityActing
    {
        add => _scheduler.EntityActing += value;
        remove => _scheduler.EntityActing -= value;
    }

    public event Action<ActionExecutionRecord>? ActionExecuted
    {
        add => _scheduler.ActionExecuted += value;
        remove => _scheduler.ActionExecuted -= value;
    }

    public event Action<ISchedulerEntity>? EntityRemoved
    {
        add => _scheduler.EntityRemoved += value;
        remove => _scheduler.EntityRemoved -= value;
    }

    public void AddEntity(ISchedulerEntity entity)
        => _scheduler.AddEntity(entity);

    public void Clear()
        => _scheduler.Clear();

    public TurnResult ProcessNextTurn()
        => _scheduler.ProcessNextTurn();

    public void ClearPendingPlayerAction()
        => _scheduler.ClearPendingPlayerAction();

    public void EnqueuePlayerAction(ISchedulerAction action)
        => _scheduler.EnqueuePlayerAction(action);

    public IReadOnlyList<ISchedulerEntity> GetEntitiesByEnergy()
        => _scheduler.GetEntitiesByEnergy();

    public ISchedulerEntity? GetPlayer()
        => _scheduler.GetPlayer();

    public TurnResult ProcessUntilPlayerInput(int maxIterations = 10000)
        => _scheduler.ProcessUntilPlayerInput(maxIterations);

    public void RemoveEntity(ISchedulerEntity entity)
        => _scheduler.RemoveEntity(entity);

    public void RemoveEntity(Guid entityId)
        => _scheduler.RemoveEntity(entityId);

    public void Reset()
        => _scheduler.Reset();
}
