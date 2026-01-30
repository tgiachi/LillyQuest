using LillyQuest.Core.Primitives;
using LillyQuest.Core.Types;
using Silk.NET.Maths;

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

    /// <summary>
    /// Flip flags applied to the foreground tile.
    /// </summary>
    public TileFlipType Flip { get; set; }

    public TileRenderData(
        int tileIndex,
        LyColor foregroundColor,
        LyColor? backgroundColor = null,
        TileFlipType flip = TileFlipType.None
    )
    {
        TileIndex = tileIndex;
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor ?? new LyColor(0, 0, 0, 0); // Transparent by default
        Flip = flip;
    }

    public static Rectangle<float> ApplyFlip(Rectangle<float> uvRect, TileFlipType flip)
    {
        var originX = uvRect.Origin.X;
        var originY = uvRect.Origin.Y;
        var sizeX = uvRect.Size.X;
        var sizeY = uvRect.Size.Y;

        if (flip.HasFlag(TileFlipType.FlipHorizontal))
        {
            originX += sizeX;
            sizeX = -sizeX;
        }

        if (flip.HasFlag(TileFlipType.FlipVertical))
        {
            originY += sizeY;
            sizeY = -sizeY;
        }

        return new(originX, originY, sizeX, sizeY);
    }

    /// <summary>
    /// Returns a darkened version of this tile by the specified factor.
    /// </summary>
    /// <param name="factor">Factor between 0.0 (black) and 1.0 (unchanged).</param>
    public TileRenderData Darken(float factor)
        => new(
            TileIndex,
            ForegroundColor.Darken(factor),
            BackgroundColor.Darken(factor),
            Flip
        );

    public override string ToString()
        => $"Tile {TileIndex}: BG={BackgroundColor} FG={ForegroundColor} Flip={Flip}";
}
