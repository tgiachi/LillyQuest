using LillyQuest.Core.Data.Json.Assets;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using Silk.NET.Maths;

namespace LillyQuest.Core.Data.Assets.Tiles;

/// <summary>
/// Represents a loaded tileset with its texture and configuration.
/// A tileset is a grid of fixed-size tiles in a single texture.
/// </summary>
public class Tileset : IDisposable
{
    public string ImagePath { get; }
    public int TileWidth { get; }
    public int TileHeight { get; }
    public int Spacing { get; }
    public int Margin { get; }
    public Texture2D Texture { get; }

    // Cached calculations
    private int? _cachedTilesPerRow;
    private int? _cachedTilesPerColumn;

    public Tileset(string imagePath, int tileWidth, int tileHeight, int spacing, int margin, Texture2D texture)
    {
        ImagePath = imagePath;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        Spacing = spacing;
        Margin = margin;
        Texture = texture;
    }

    /// <summary>
    /// Number of tiles per row in the tileset grid.
    /// </summary>
    public int TilesPerRow
    {
        get
        {
            _cachedTilesPerRow ??= (Texture.Width - Margin * 2 + Spacing) / (TileWidth + Spacing);

            return _cachedTilesPerRow.Value;
        }
    }

    /// <summary>
    /// Number of tiles per column in the tileset grid.
    /// </summary>
    public int TilesPerColumn
    {
        get
        {
            _cachedTilesPerColumn ??= (Texture.Height - Margin * 2 + Spacing) / (TileHeight + Spacing);

            return _cachedTilesPerColumn.Value;
        }
    }

    /// <summary>
    /// Total number of tiles in the tileset.
    /// </summary>
    public int TileCount => TilesPerRow * TilesPerColumn;

    public void Dispose()
    {
        Texture?.Dispose();
        GC.SuppressFinalize(this);
    }

    public static Tileset FromDefinition(SpriteSheetDefinitionJson definition, Texture2D texture)
        => new(
            definition.ImagePath,
            definition.TileWidth,
            definition.TileHeight,
            definition.Spacing,
            definition.Margin,
            texture
        );

    /// <summary>
    /// Gets tile data based on grid coordinates (x, y).
    /// </summary>
    /// <param name="tileX">Column of the tile (0-based)</param>
    /// <param name="tileY">Row of the tile (0-based)</param>
    /// <returns>TileData struct containing the source rectangle for SpriteBatch</returns>
    /// <exception cref="ArgumentOutOfRangeException">If coordinates are out of bounds</exception>
    public TileData GetTile(int tileX, int tileY)
    {
        if (tileX < 0 || tileX >= TilesPerRow)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tileX),
                $"Tile X coordinate must be between 0 and {TilesPerRow - 1}"
            );
        }

        if (tileY < 0 || tileY >= TilesPerColumn)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tileY),
                $"Tile Y coordinate must be between 0 and {TilesPerColumn - 1}"
            );
        }

        // Calculate tile position in texture considering margin and spacing
        var sourceX = Margin + tileX * (TileWidth + Spacing);
        var sourceY = Margin + tileY * (TileHeight + Spacing);

        var sourceRect = new Rectangle<int>(sourceX, sourceY, TileWidth, TileHeight);

        var index = tileY * TilesPerRow + tileX;

        return new(tileX, tileY, index, sourceRect);
    }

    /// <summary>
    /// Gets tile data based on linear index (0-based, row-major order).
    /// </summary>
    /// <param name="index">Linear index of the tile</param>
    /// <returns>TileData struct containing the source rectangle for SpriteBatch</returns>
    /// <exception cref="ArgumentOutOfRangeException">If index is out of bounds</exception>
    public TileData GetTile(int index)
    {
        if (index < 0 || index >= TileCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"Tile index must be between 0 and {TileCount - 1}");
        }

        var tileX = index % TilesPerRow;
        var tileY = index / TilesPerRow;

        return GetTile(tileX, tileY);
    }

    public override string ToString()
        => $"Tileset: {ImagePath} ({TileWidth}x{TileHeight}, Spacing: {Spacing}, Margin: {Margin}) - {TileCount} tiles";
}
