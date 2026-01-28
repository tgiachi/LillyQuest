using LillyQuest.Core.Primitives;

namespace LillyQuest.RogueLike.Components;

public sealed class LightBackgroundComponent
{
    public LyColor StartBackground { get; }
    public LyColor EndBackground { get; }

    public LightBackgroundComponent(LyColor startBackground, LyColor endBackground)
    {
        StartBackground = startBackground;
        EndBackground = endBackground;
    }
}
