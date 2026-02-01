using LillyQuest.Core.Primitives;

namespace LillyQuest.RogueLike.Components;

/// <summary>
/// Component that defines torch lighting for foreground and background.
/// </summary>
public sealed class TorchComponent
{
    public int Radius { get; }
    public LyColor ForegroundStart { get; }
    public LyColor ForegroundEnd { get; }
    public LyColor BackgroundStart { get; }
    public LyColor BackgroundEnd { get; }
    public byte BackgroundAlpha { get; }

    public TorchComponent(
        int radius,
        LyColor foregroundStart,
        LyColor foregroundEnd,
        LyColor backgroundStart,
        LyColor backgroundEnd,
        byte backgroundAlpha
    )
    {
        Radius = radius;
        ForegroundStart = foregroundStart;
        ForegroundEnd = foregroundEnd;
        BackgroundStart = backgroundStart;
        BackgroundEnd = backgroundEnd;
        BackgroundAlpha = backgroundAlpha;
    }
}
