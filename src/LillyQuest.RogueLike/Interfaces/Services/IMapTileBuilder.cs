using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Systems;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.Interfaces.Services;

/// <summary>
/// Service responsible for building tile render data from map game objects.
/// Converts game objects and terrain at a position into renderable tile data,
/// applying FOV visibility and darkening effects.
/// </summary>
public interface IMapTileBuilder
{
    /// <summary>
    /// Builds a creature tile for the given position on the map.
    /// Returns empty tile if no creature is present or if not visible.
    /// </summary>
    /// <param name="map">The map to build the tile from.</param>
    /// <param name="fovSystem">Optional FOV system to determine visibility and darkening.</param>
    /// <param name="position">The position to build the tile for.</param>
    /// <returns>The built tile data, or empty tile if not renderable.</returns>
    TileRenderData BuildCreatureTile(LyQuestMap map, FovSystem? fovSystem, Point position);

    /// <summary>
    /// Builds an item tile for the given position on the map.
    /// Returns empty tile if no item is present or if not visible.
    /// </summary>
    /// <param name="map">The map to build the tile from.</param>
    /// <param name="fovSystem">Optional FOV system to determine visibility and darkening.</param>
    /// <param name="position">The position to build the tile for.</param>
    /// <returns>The built tile data, or empty tile if not renderable.</returns>
    TileRenderData BuildItemTile(LyQuestMap map, FovSystem? fovSystem, Point position);

    /// <summary>
    /// Builds a terrain tile for the given position on the map.
    /// Applies FOV-based visibility logic: unexplored tiles are hidden,
    /// visible tiles are full brightness, explored-but-not-visible tiles are darkened.
    /// </summary>
    /// <param name="map">The map to build the tile from.</param>
    /// <param name="fovSystem">Optional FOV system to determine visibility and exploration state.</param>
    /// <param name="position">The position to build the tile for.</param>
    /// <returns>The built tile data.</returns>
    TileRenderData BuildTerrainTile(LyQuestMap map, FovSystem? fovSystem, Point position);
}
