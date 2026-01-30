using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Systems;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.Services;

/// <summary>
/// Service that builds tile render data from game objects on a map.
/// Stateless service that converts positions and game objects into TileRenderData,
/// applying FOV visibility and darkening effects when a FOV system is provided.
/// </summary>
public sealed class MapTileBuilder : IMapTileBuilder
{
    public TileRenderData BuildCreatureTile(LyQuestMap map, FovSystem? fovSystem, Point position)
    {
        var renderCreature = fovSystem == null || fovSystem.IsVisible(map, position);
        var empty = new TileRenderData(-1, LyColor.White);

        if (!renderCreature)
        {
            return empty;
        }

        foreach (var obj in map.GetObjectsAt(position))
        {
            if (obj is CreatureGameObject creature)
            {
                var tile = new TileRenderData(
                    creature.Tile.Symbol[0],
                    creature.Tile.ForegroundColor,
                    creature.Tile.BackgroundColor,
                    creature.Tile.Flip
                );

                if (fovSystem != null)
                {
                    tile = tile.Darken(fovSystem.GetVisibilityFalloff(map, position));
                }

                return tile;
            }
        }

        return empty;
    }

    public TileRenderData BuildItemTile(LyQuestMap map, FovSystem? fovSystem, Point position)
    {
        var renderItem = fovSystem == null || fovSystem.IsVisible(map, position);
        var empty = new TileRenderData(-1, LyColor.White);

        if (!renderItem)
        {
            return empty;
        }

        foreach (var obj in map.GetObjectsAt(position))
        {
            if (obj is ItemGameObject item)
            {
                var tile = new TileRenderData(
                    item.Tile.Symbol[0],
                    item.Tile.ForegroundColor,
                    item.Tile.BackgroundColor,
                    item.Tile.Flip
                );

                if (fovSystem != null)
                {
                    tile = tile.Darken(fovSystem.GetVisibilityFalloff(map, position));
                }

                return tile;
            }
        }

        return empty;
    }

    public TileRenderData BuildTerrainTile(LyQuestMap map, FovSystem? fovSystem, Point position)
    {
        var tile = new TileRenderData(-1, LyColor.White);

        if (map.GetTerrainAt(position) is TerrainGameObject terrain)
        {
            tile = new(
                terrain.Tile.Symbol[0],
                terrain.Tile.ForegroundColor,
                terrain.Tile.BackgroundColor,
                terrain.Tile.Flip
            );
        }

        if (fovSystem == null)
        {
            return tile;
        }

        var isVisible = fovSystem.IsVisible(map, position);
        var isExplored = fovSystem.IsExplored(map, position);

        if (!isExplored)
        {
            return new(-1, LyColor.White);
        }

        if (!isVisible && isExplored)
        {
            return tile.Darken(0.5f);
        }

        return tile.Darken(fovSystem.GetVisibilityFalloff(map, position));
    }
}
