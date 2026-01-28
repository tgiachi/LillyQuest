namespace LillyQuest.RogueLike.Interfaces.Scheduler;

/// <summary>
/// An entity that participates in the turn-based scheduler system.
/// Energy accumulates each tick based on Speed. When energy >= threshold,
/// the entity can act.
/// </summary>
public interface ISchedulerEntity
{
    /// <summary>
    /// Unique identifier for scheduling.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Current energy. When >= Scheduler.EnergyThreshold (100), entity can act.
    /// </summary>
    int Energy { get; set; }

    /// <summary>
    /// Energy gained per tick. Higher = acts more frequently.
    /// Typical values: 8 (slow), 10 (normal), 12 (fast), 15 (very fast).
    /// </summary>
    int Speed { get; }

    /// <summary>
    /// Whether this entity is the player (pauses for input).
    /// </summary>
    bool IsPlayer { get; }

    /// <summary>
    /// Whether this entity is still active in the scheduler.
    /// Dead/removed entities should return false.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Gets the next action this entity wants to perform.
    /// For AI: returns computed action immediately.
    /// For Player: returns null (use EnqueuePlayerAction instead).
    /// </summary>
    ISchedulerAction? GetNextAction();
}
