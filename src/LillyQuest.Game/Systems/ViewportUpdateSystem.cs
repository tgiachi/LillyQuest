using System.Numerics;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Game.Systems;

public readonly record struct TileViewportBounds(int MinX, int MinY, int MaxX, int MaxY);

public sealed class ViewportUpdateSystem
{
    public static TileViewportBounds GetViewportBounds(TilesetSurfaceScreen screen, int layerIndex)
    {
        var offset = screen.GetLayerViewTileOffset(layerIndex);
        var minX = (int)MathF.Floor(offset.X);
        var minY = (int)MathF.Floor(offset.Y);
        var maxX = minX + (int)screen.TileViewSize.X - 1;
        var maxY = minY + (int)screen.TileViewSize.Y - 1;

        return new(minX, minY, maxX, maxY);
    }
}
