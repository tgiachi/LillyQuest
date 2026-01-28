using System.Numerics;

namespace LillyQuest.Engine.Screens.TilesetSurface;

/// <summary>
/// Context for rendering a tileset surface.
/// </summary>
internal sealed class TilesetSurfaceRenderContext
{
    public float TileRenderScale { get; set; } = 1.0f;
    public Vector2 ScreenPosition { get; set; }
    public Vector2 ScreenSize { get; set; }
    public Vector4 Margin { get; set; }
}
