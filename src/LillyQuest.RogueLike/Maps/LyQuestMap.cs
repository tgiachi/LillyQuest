using System.Runtime.CompilerServices;
using GoRogue.GameFramework;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;
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

            foreach (var entity in GetObjectsAt(position))
            {
                if (entity is CreatureGameObject creature)
                {
                    surface.AddTileToSurface(
                        creature.Layer,
                        position.X,
                        position.Y,
                        new TileRenderData(
                            creature.Tile.Symbol[0],
                            creature.Tile.ForegroundColor
                        )
                    );
                }
            }
        }
        var player = Entities.GetLayer((int)MapLayer.Creatures).First();

        surface.CenterViewOnTile(0, player.Position.X, player.Position.Y);
    }
}
