using System.Numerics;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Engine.Screens.TilesetSurface;

/// <summary>
/// Represents a single layer in a tileset surface for drawing/editing.
/// </summary>
public class TileLayer
{
    /// <summary>
    /// 2D array of tile render data. Tiles with index -1 are considered empty.
    /// </summary>
    public TileRenderData[,] Tiles { get; set; }

    /// <summary>
    /// Opacity of this layer (0.0 = invisible, 1.0 = fully opaque).
    /// </summary>
    public float Opacity { get; set; } = 1.0f;

    /// <summary>
    /// Whether this layer is visible.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Name of the tileset to use for this layer.
    /// If null or empty, the default tileset is used.
    /// </summary>
    public string? TilesetName { get; set; }

    /// <summary>
    /// Pixel offset applied when rendering this layer.
    /// </summary>
    public Vector2 PixelOffset { get; set; } = Vector2.Zero;

    /// <summary>
    /// View offset in tile coordinates for this layer.
    /// </summary>
    public Vector2 ViewTileOffset { get; set; } = Vector2.Zero;

    /// <summary>
    /// Target view offset in tile coordinates for smooth scrolling.
    /// </summary>
    public Vector2 ViewTileOffsetTarget { get; set; } = Vector2.Zero;

    /// <summary>
    /// View offset in pixels for this layer (for smooth scrolling).
    /// </summary>
    public Vector2 ViewPixelOffset { get; set; } = Vector2.Zero;

    /// <summary>
    /// Target view offset in pixels for smooth scrolling.
    /// </summary>
    public Vector2 ViewPixelOffsetTarget { get; set; } = Vector2.Zero;

    /// <summary>
    /// Enables smooth scrolling for this layer view.
    /// </summary>
    public bool SmoothViewEnabled { get; set; }

    /// <summary>
    /// Smooth view speed in units per second.
    /// </summary>
    public float SmoothViewSpeed { get; set; } = 10f;

    /// <summary>
    /// Optional override for input tile size (in pixels).
    /// If set, mouse-to-tile conversion uses this instead of the tileset size.
    /// </summary>
    public Vector2? InputTileSizeOverride { get; set; }

    public TileLayer(int width, int height)
    {
        Tiles = new TileRenderData[width, height];

        // Initialize with empty tiles (index -1)
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                Tiles[x, y] = new(-1, LyColor.White);
            }
        }
    }
}
