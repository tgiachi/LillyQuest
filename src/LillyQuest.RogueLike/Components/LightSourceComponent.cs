using LillyQuest.Core.Primitives;

namespace LillyQuest.RogueLike.Components;

public sealed class LightSourceComponent
{
    public int Radius { get; }
    public LyColor StartColor { get; }
    public LyColor EndColor { get; }

    public LightSourceComponent(int radius, LyColor startColor, LyColor endColor)
    {
        Radius = radius;
        StartColor = startColor;
        EndColor = endColor;
    }
}
