using LillyQuest.Core.Primitives;

namespace LillyQuest.Core.Data.Assets.Tiles;

/// <summary>
/// Represents rendering data for a tile including background and foreground colors.
/// </summary>
public struct TileRenderData
{
    /// <summary>
    /// Index of the tile to render.
    /// </summary>
    public int TileIndex { get; set; }

    /// <summary>
    /// Background color (solid). If set to transparent (A=0), background is not drawn.
    /// </summary>
    public LyColor BackgroundColor { get; set; }

    /// <summary>
    /// Foreground color tint applied to the tile.
    /// </summary>
    public LyColor ForegroundColor { get; set; }

    public TileRenderData(int tileIndex, LyColor foregroundColor, LyColor? backgroundColor = null)
    {
        TileIndex = tileIndex;
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor ?? new LyColor(0, 0, 0, 0);  // Transparent by default
    }

    public override string ToString()
        => $"Tile {TileIndex}: BG={BackgroundColor} FG={ForegroundColor}";
}
