using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Systems;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.Services;

/// <summary>
/// Service that builds tile render data from game objects on a map.
/// Stateless service that converts positions and game objects into TileRenderData,
/// applying FOV visibility and darkening effects when a FOV system is provided.
/// </summary>
public sealed class MapTileBuilder : IMapTileBuilder
{
    private const int TorchRadius = 4;
    private const float MaxTorchBrightness = 1.4f;

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

        tile = tile.Darken(fovSystem.GetVisibilityFalloff(map, position));

        // Apply torch light effect to non-transparent terrain
        var terrainAtPosition = map.GetTerrainAt(position);
        if (terrainAtPosition is not null && !terrainAtPosition.IsTransparent)
        {
            var playerEntity = map.Entities.GetLayer((int)MapLayer.Creatures).FirstOrDefault();
            if (playerEntity != null)
            {
                var playerPosition = playerEntity.Position;
                var distance = Distance.Euclidean.Calculate(playerPosition, position);

                if (distance <= TorchRadius)
                {
                    var brightnessMultiplier = CalculateTorchBrightness(distance, TorchRadius, MaxTorchBrightness);
                    tile = tile.Brighten(brightnessMultiplier);
                }
            }
        }

        return tile;
    }

    /// <summary>
    /// Calculate torch brightness based on distance from player.
    /// </summary>
    /// <param name="distance">Distance from player to tile.</param>
    /// <param name="radius">Maximum torch radius.</param>
    /// <param name="maxBrightness">Maximum brightness multiplier at player position.</param>
    /// <returns>Brightness factor (1.0 = no change, > 1.0 = brighter).</returns>
    private static float CalculateTorchBrightness(double distance, int radius, float maxBrightness)
    {
        // Linear falloff: brightness decreases linearly with distance
        // At distance 0 (player position): maxBrightness
        // At distance radius: 1.0 (no brightening)
        var normalizedDistance = (float)(distance / radius);
        var brightnessRange = maxBrightness - 1f;
        return 1f + brightnessRange * (1f - normalizedDistance);
    }
}
