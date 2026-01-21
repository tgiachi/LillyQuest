namespace LillyQuest.Game.Screens.TilesetSurface;

/// <summary>
/// Represents a 2D surface for drawing tiles across multiple layers.
/// </summary>
public class TilesetSurface
{
    /// <summary>
    /// All layers in the surface, ordered from bottom (index 0) to top (index N-1).
    /// </summary>
    public List<TileLayer> Layers { get; } = new();

    /// <summary>
    /// Width of the surface in tiles.
    /// </summary>
    public int Width { get; set; } = 50;

    /// <summary>
    /// Height of the surface in tiles.
    /// </summary>
    public int Height { get; set; } = 50;

    /// <summary>
    /// Gets a tile at the given coordinates on the specified layer.
    /// </summary>
    public int GetTile(int layerIndex, int x, int y)
    {
        if (layerIndex < 0 || layerIndex >= Layers.Count)
        {
            return -1;
        }

        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return -1;
        }

        return Layers[layerIndex].TileIndices[x, y];
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
    public void SetTile(int layerIndex, int x, int y, int tileIndex)
    {
        if (layerIndex < 0 || layerIndex >= Layers.Count)
        {
            return;
        }

        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return;
        }

        Layers[layerIndex].TileIndices[x, y] = tileIndex;
    }
}
