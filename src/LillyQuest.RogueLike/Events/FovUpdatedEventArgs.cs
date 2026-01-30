using LillyQuest.RogueLike.Maps;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.Events;

/// <summary>
/// Event arguments for when the field of view has been updated.
/// </summary>
public sealed class FovUpdatedEventArgs : EventArgs
{
    /// <summary>
    /// The map on which the FOV was updated.
    /// </summary>
    public LyQuestMap Map { get; }

    /// <summary>
    /// Tiles that were visible before the update.
    /// </summary>
    public IReadOnlySet<Point> PreviousVisibleTiles { get; }

    /// <summary>
    /// Tiles that are currently visible after the update.
    /// </summary>
    public IReadOnlySet<Point> CurrentVisibleTiles { get; }

    public FovUpdatedEventArgs(
        LyQuestMap map,
        IReadOnlySet<Point> previousVisibleTiles,
        IReadOnlySet<Point> currentVisibleTiles
    )
    {
        Map = map;
        PreviousVisibleTiles = previousVisibleTiles;
        CurrentVisibleTiles = currentVisibleTiles;
    }
}
