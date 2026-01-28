using System.Numerics;

namespace LillyQuest.Engine.Screens.TilesetSurface;

/// <summary>
/// Handles mouse-to-tile coordinate conversion for tileset surfaces.
/// </summary>
internal sealed class TilesetSurfaceInputHandler
{
    private readonly TilesetSurfaceInputContext _context;

    public TilesetSurfaceInputHandler(TilesetSurfaceInputContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Converts mouse coordinates to tile coordinates.
    /// </summary>
    public (int x, int y) GetInputTileCoordinates(
        float layerRenderScale,
        int tileWidth,
        int tileHeight,
        Vector2 layerPixelOffset,
        Vector2 viewTileOffset,
        Vector2 viewPixelOffset,
        int mouseX,
        int mouseY
    )
    {
        var scaledTileWidth = tileWidth * _context.TileRenderScale * layerRenderScale;
        var scaledTileHeight = tileHeight * _context.TileRenderScale * layerRenderScale;

        var viewOffsetPx = new Vector2(
                               viewTileOffset.X * scaledTileWidth,
                               viewTileOffset.Y * scaledTileHeight
                           ) +
                           viewPixelOffset;

        var relativeX = mouseX - _context.ScreenPosition.X - layerPixelOffset.X + viewOffsetPx.X;
        var relativeY = mouseY - _context.ScreenPosition.Y - layerPixelOffset.Y + viewOffsetPx.Y;

        var tileX = (int)MathF.Floor(relativeX / scaledTileWidth);
        var tileY = (int)MathF.Floor(relativeY / scaledTileHeight);

        return (tileX, tileY);
    }

    /// <summary>
    /// Tests if a point is within the screen bounds.
    /// </summary>
    public bool HitTest(int x, int y, Vector2 screenSize)
    {
        var left = _context.ScreenPosition.X;
        var top = _context.ScreenPosition.Y;
        var right = left + screenSize.X;
        var bottom = top + screenSize.Y;

        return x >= left && x < right && y >= top && y < bottom;
    }
}
