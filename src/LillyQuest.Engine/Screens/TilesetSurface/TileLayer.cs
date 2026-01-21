using System.Numerics;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Engine.Screens.TilesetSurface;

/// <summary>
/// Represents a single layer in a tileset surface for drawing/editing.
/// </summary>
public class TileLayer
{
    private readonly Dictionary<(int x, int y), TileChunk> _chunks = new();

    public int Width { get; }
    public int Height { get; }

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

    /// <summary>
    /// Visual render scale multiplier for this layer.
    /// </summary>
    public float RenderScale { get; set; } = 1f;

    /// <summary>
    /// Target render scale for smooth zoom.
    /// </summary>
    public float RenderScaleTarget { get; set; } = 1f;

    /// <summary>
    /// Enables smooth render scale transitions.
    /// </summary>
    public bool SmoothRenderScaleEnabled { get; set; }

    /// <summary>
    /// Smooth render scale speed in units per second.
    /// </summary>
    public float SmoothRenderScaleSpeed { get; set; } = 10f;

    public TileLayer(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public int GetChunkCount() => _chunks.Count;

    public TileRenderData GetTile(int x, int y)
    {
        var (chunkX, chunkY, localX, localY) = ToChunkCoordinates(x, y);

        if (!_chunks.TryGetValue((chunkX, chunkY), out var chunk))
        {
            return new(-1, LyColor.White);
        }

        return chunk.GetTile(localX, localY);
    }

    public void SetTile(int x, int y, TileRenderData tileData)
    {
        var (chunkX, chunkY, localX, localY) = ToChunkCoordinates(x, y);

        if (!_chunks.TryGetValue((chunkX, chunkY), out var chunk))
        {
            chunk = new TileChunk();
            _chunks[(chunkX, chunkY)] = chunk;
        }

        chunk.SetTile(localX, localY, tileData);
    }

    private static (int chunkX, int chunkY, int localX, int localY) ToChunkCoordinates(int x, int y)
    {
        var chunkX = x / TileChunk.Size;
        var chunkY = y / TileChunk.Size;
        var localX = x % TileChunk.Size;
        var localY = y % TileChunk.Size;

        return (chunkX, chunkY, localX, localY);
    }
}
