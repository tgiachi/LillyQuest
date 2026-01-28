using System.Numerics;

namespace LillyQuest.Engine.Screens.TilesetSurface;

/// <summary>
/// Context data needed for input handling calculations.
/// </summary>
internal sealed class TilesetSurfaceInputContext
{
    public float TileRenderScale { get; set; } = 1.0f;
    public Vector2 ScreenPosition { get; set; }
    public Vector4 Margin { get; set; }
}
