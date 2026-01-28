using LillyQuest.Core.Primitives;

namespace LillyQuest.RogueLike.Components;

/// <summary>
/// Component that defines the foreground light gradient for a light-emitting entity.
/// </summary>
public sealed class LightSourceComponent
{
    /// <summary>
    /// The radius of the light in tiles.
    /// </summary>
    public int Radius { get; }

    /// <summary>
    /// Foreground color at the light origin.
    /// </summary>
    public LyColor StartColor { get; }

    /// <summary>
    /// Foreground color at the edge of the light radius.
    /// </summary>
    public LyColor EndColor { get; }

    public LightSourceComponent(int radius, LyColor startColor, LyColor endColor)
    {
        Radius = radius;
        StartColor = startColor;
        EndColor = endColor;
    }
}
