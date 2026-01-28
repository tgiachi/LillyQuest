using LillyQuest.Core.Primitives;

namespace LillyQuest.RogueLike.Components;

/// <summary>
/// Component that defines the background light gradient for overlay rendering.
/// </summary>
public sealed class LightBackgroundComponent
{
    /// <summary>
    /// Background color at the light origin.
    /// </summary>
    public LyColor StartBackground { get; }

    /// <summary>
    /// Background color at the edge of the light radius.
    /// </summary>
    public LyColor EndBackground { get; }

    public LightBackgroundComponent(LyColor startBackground, LyColor endBackground)
    {
        StartBackground = startBackground;
        EndBackground = endBackground;
    }
}
