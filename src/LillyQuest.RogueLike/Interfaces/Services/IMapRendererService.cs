using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.RogueLike.Maps;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.Interfaces.Services;

/// <summary>
/// Service responsible for rendering a map to a surface.
/// Separates rendering logic from map data model.
/// </summary>
public interface IMapRendererService
{
    /// <summary>
    /// Renders the entire map to the surface.
    /// </summary>
    /// <param name="map">The map to render.</param>
    /// <param name="surface">The surface to render to.</param>
    void RenderMap(LyQuestMap map, TilesetSurfaceScreen surface);

    /// <summary>
    /// Renders the map and centers the view on a specific position.
    /// </summary>
    /// <param name="map">The map to render.</param>
    /// <param name="surface">The surface to render to.</param>
    /// <param name="centerOn">The position to center the view on.</param>
    void RenderMap(LyQuestMap map, TilesetSurfaceScreen surface, Point centerOn);

    /// <summary>
    /// Renders only a specific region of the map (for optimization).
    /// </summary>
    /// <param name="map">The map to render.</param>
    /// <param name="surface">The surface to render to.</param>
    /// <param name="region">The region to render.</param>
    void RenderRegion(LyQuestMap map, TilesetSurfaceScreen surface, Rectangle region);

    /// <summary>
    /// Updates a single tile on the surface (for incremental updates).
    /// </summary>
    /// <param name="map">The map containing the tile.</param>
    /// <param name="surface">The surface to update.</param>
    /// <param name="position">The position of the tile to update.</param>
    void UpdateTile(LyQuestMap map, TilesetSurfaceScreen surface, Point position);
}
