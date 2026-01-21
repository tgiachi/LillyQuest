using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Engine.Screens.TilesetSurface;

/// <summary>
/// Represents a 2D surface for drawing tiles across multiple layers.
/// </summary>
public class TilesetSurface
{
    /// <summary>
    /// All layers in the surface, ordered from bottom (index 0) to top (index N-1).
    /// </summary>
    public List<TileLayer> Layers { get; } = [];

    /// <summary>
    /// Width of the surface in tiles.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Height of the surface in tiles.
    /// </summary>
    public int Height { get; set; }

    public TilesetSurface(int width = 100, int height = 100)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Gets a tile at the given coordinates on the specified layer.
    /// </summary>
    public TileRenderData GetTile(int layerIndex, int x, int y)
    {
        if (layerIndex < 0 || layerIndex >= Layers.Count)
        {
            return new(-1, LyColor.White);
        }

        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return new(-1, LyColor.White);
        }

        return Layers[layerIndex].GetTile(x, y);
    }

    /// <summary>
    /// Initialize the surface with a specific number of layers.
    /// </summary>
    public void Initialize(int layerCount)
    {
        Layers.Clear();

        for (var i = 0; i < layerCount; i++)
        {
            Layers.Add(new(Width, Height));
        }
    }

    /// <summary>
    /// Sets a tile at the given coordinates on the specified layer.
    /// </summary>
    public void SetTile(int layerIndex, int x, int y, TileRenderData tileData)
    {
        if (layerIndex < 0 || layerIndex >= Layers.Count)
        {
            return;
        }

        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return;
        }

        Layers[layerIndex].SetTile(x, y, tileData);
    }

    /// <summary>
    /// Handles a mouse wheel delta over a specific tile and returns the delta if valid.
    /// </summary>
    public float HandleMouseWheel(int layerIndex, int x, int y, float delta)
    {
        if (layerIndex < 0 || layerIndex >= Layers.Count)
        {
            return 0f;
        }

        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return 0f;
        }

        return delta;
    }
}
