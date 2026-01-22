using System.Numerics;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Engine.Types;

namespace LillyQuest.Engine.Screens.TilesetSurface;

/// <summary>
/// Represents a single tile movement animation.
/// </summary>
public sealed class TileMovement
{
    public int LayerIndex { get; }
    public Vector2 SourceTile { get; }
    public Vector2 DestinationTile { get; }
    public bool Bounce { get; }
    public float DurationSeconds { get; }
    public float ElapsedSeconds { get; set; }
    public TileRenderData TileData { get; }
    public TileMovementState State { get; set; }

    public TileMovement(
        int layerIndex,
        Vector2 sourceTile,
        Vector2 destinationTile,
        bool bounce,
        float durationSeconds,
        TileRenderData tileData
    )
    {
        LayerIndex = layerIndex;
        SourceTile = sourceTile;
        DestinationTile = destinationTile;
        Bounce = bounce;
        DurationSeconds = durationSeconds;
        TileData = tileData;
        State = TileMovementState.Pending;
    }
}
