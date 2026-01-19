using Silk.NET.Maths;

namespace LillyQuest.Core.Data.Assets.Tiles;

/// <summary>
/// Represents the data needed to render a single tile from a tileset.
/// Contains the source rectangle of the texture that corresponds to the tile.
/// </summary>
public readonly struct TileData
{
    /// <summary>
    /// Source rectangle in the texture that represents this tile.
    /// Used by SpriteBatch.Draw() to specify which part of the texture to draw.
    /// </summary>
    public Rectangle<int> SourceRect { get; }

    /// <summary>
    /// Index of the tile in the tileset (0-based).
    /// Useful for references or debugging.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// X coordinate (column) of the tile in the tileset grid.
    /// </summary>
    public int TileX { get; }

    /// <summary>
    /// Y coordinate (row) of the tile in the tileset grid.
    /// </summary>
    public int TileY { get; }

    public TileData(int tileX, int tileY, int index, Rectangle<int> sourceRect)
    {
        TileX = tileX;
        TileY = tileY;
        Index = index;
        SourceRect = sourceRect;
    }

    public override string ToString()
        => $"Tile({TileX}, {TileY}) [index: {Index}] - Source: {SourceRect.Origin.X}, {SourceRect.Origin.Y} ({SourceRect.Size.X}x{SourceRect.Size.Y})";
}
