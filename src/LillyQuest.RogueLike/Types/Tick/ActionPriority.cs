namespace LillyQuest.RogueLike.Types.Tick;

/// <summary>
///     Priority levels for action execution order
/// </summary>
public enum ActionPriority : byte
{
    /// <summary>
    ///     Instant actions that don't consume a turn (reactions, UI, looking)
    /// </summary>
    Instant = 0,

    /// <summary>
    ///     High priority actions (defensive reactions, critical responses)
    /// </summary>
    High = 1,

    /// <summary>
    ///     Normal priority actions (movement, attacks, item use)
    /// </summary>
    Normal = 2,

    /// <summary>
    ///     Low priority actions (passive abilities, regeneration)
    /// </summary>
    Low = 3,

    /// <summary>
    ///     Environmental actions (world events, ambient effects)
    /// </summary>
    Environmental = 4
}
