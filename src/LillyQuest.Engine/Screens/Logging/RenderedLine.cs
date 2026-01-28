using LillyQuest.Engine.Logging;

namespace LillyQuest.Engine.Screens.Logging;

public sealed class RenderedLine
{
    public IReadOnlyList<StyledSpan> Spans { get; set; }
    public float BlinkRemaining { get; set; }

    public RenderedLine(IReadOnlyList<StyledSpan> spans, float blinkRemaining)
    {
        Spans = spans;
        BlinkRemaining = blinkRemaining;
    }
}
