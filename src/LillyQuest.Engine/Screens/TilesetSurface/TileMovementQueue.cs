namespace LillyQuest.Engine.Screens.TilesetSurface;

/// <summary>
/// Holds pending and active tile movements for a layer.
/// </summary>
public sealed class TileMovementQueue
{
    public Queue<TileMovement> Pending { get; } = new();
    public List<TileMovement> Active { get; } = new();
}
