namespace LillyQuest.Game.Screens.TilesetSurface;

/// <summary>
/// Represents a single layer in a tileset surface for drawing/editing.
/// </summary>
public class TileLayer
{
    /// <summary>
    /// 2D array of tile indices. -1 or 0 means empty.
    /// </summary>
    public int[,] TileIndices { get; set; }

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

    public TileLayer(int width, int height)
    {
        TileIndices = new int[width, height];
        // Initialize with -1 (empty)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileIndices[x, y] = -1;
            }
        }
    }
}
