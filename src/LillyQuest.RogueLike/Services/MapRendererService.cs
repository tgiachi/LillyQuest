using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace LillyQuest.RogueLike.Services;

/// <summary>
/// Service responsible for rendering a map to a surface.
/// </summary>
public sealed class MapRendererService : IMapRendererService
{
    private const int TerrainLayer = 0;

    /// <inheritdoc />
    public void RenderMap(LyQuestMap map, TilesetSurfaceScreen surface)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(surface);

        RenderAllTiles(map, surface);
        CenterOnPlayer(map, surface);
    }

    /// <inheritdoc />
    public void RenderMap(LyQuestMap map, TilesetSurfaceScreen surface, Point centerOn)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(surface);

        RenderAllTiles(map, surface);
        surface.CenterViewOnTile(TerrainLayer, centerOn.X, centerOn.Y);
    }

    /// <inheritdoc />
    public void RenderRegion(LyQuestMap map, TilesetSurfaceScreen surface, Rectangle region)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(surface);

        foreach (var position in region.Positions())
        {
            if (position.X < 0 || position.X >= map.Width ||
                position.Y < 0 || position.Y >= map.Height)
            {
                continue;
            }

            RenderTileAt(map, surface, position);
        }
    }

    /// <inheritdoc />
    public void UpdateTile(LyQuestMap map, TilesetSurfaceScreen surface, Point position)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(surface);

        if (position.X < 0 || position.X >= map.Width ||
            position.Y < 0 || position.Y >= map.Height)
        {
            return;
        }

        RenderTileAt(map, surface, position);
    }

    private static void RenderAllTiles(LyQuestMap map, TilesetSurfaceScreen surface)
    {
        foreach (var position in map.Positions())
        {
            RenderTileAt(map, surface, position);
        }
    }

    private static void RenderTileAt(LyQuestMap map, TilesetSurfaceScreen surface, Point position)
    {
        // Render terrain
        if (map.GetTerrainAt(position) is TerrainGameObject terrain)
        {
            surface.AddTileToSurface(
                TerrainLayer,
                position.X,
                position.Y,
                new(
                    terrain.Tile.Symbol[0],
                    terrain.Tile.ForegroundColor,
                    terrain.Tile.BackgroundColor
                )
            );
        }

        // Render entities at this position
        foreach (var entity in map.GetObjectsAt(position))
        {
            switch (entity)
            {
                case CreatureGameObject creature:
                    surface.AddTileToSurface(
                        creature.Layer,
                        position.X,
                        position.Y,
                        new(
                            creature.Tile.Symbol[0],
                            creature.Tile.ForegroundColor
                        )
                    );
                    break;

                case ItemGameObject item:
                    surface.AddTileToSurface(
                        item.Layer,
                        position.X,
                        position.Y,
                        new(
                            item.Tile.Symbol[0],
                            item.Tile.ForegroundColor,
                            item.Tile.BackgroundColor
                        )
                    );
                    break;
            }
        }
    }

    private static void CenterOnPlayer(LyQuestMap map, TilesetSurfaceScreen surface)
    {
        var creatureLayer = map.Entities.GetLayer((int)MapLayer.Creatures);

        if (creatureLayer.Count == 0)
        {
            return;
        }

        var player = creatureLayer.First();
        surface.CenterViewOnTile(TerrainLayer, player.Position.X, player.Position.Y);
    }
}
