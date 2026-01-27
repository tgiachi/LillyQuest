using GoRogue.GameFramework;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace LillyQuest.RogueLike.Maps;

public class LyQuestMap : Map
{
    public LyQuestMap(int width, int height) : base(
        width,
        height,
        Enum.GetValues<MapLayer>().Length,
        Distance.Manhattan
    ) { }

    public void FillSurface(TilesetSurfaceScreen surface)
    {
        foreach (var position in this.Positions())
        {
            if (GetTerrainAt(position) is TerrainGameObject terrain)
            {
                surface.AddTileToSurface(
                    0,
                    position.X,
                    position.Y,
                    new TileRenderData(
                        terrain.Tile.Symbol[0],
                        terrain.Tile.ForegroundColor,
                        terrain.Tile.BackgroundColor
                    )
                );
            }
        }
    }
}
