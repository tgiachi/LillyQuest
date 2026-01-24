using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Logging;

namespace LillyQuest.Tests.Engine.Logging;

public class TypewriterQueueTests
{
    [Test]
    public void Typewriter_Reveals_Characters_Sequentially()
    {
        var queue = new TypewriterQueue(charactersPerSecond: 10f);
        queue.EnqueueLine([
            new StyledSpan("Hello", LyColor.White, null, false, false, false)
        ]);

        queue.Update(TimeSpan.FromSeconds(0.1));

        Assert.That(queue.CurrentLineText, Is.EqualTo("H"));
    }

    [Test]
    public void Typewriter_Accumulates_Fractional_Progress()
    {
        var queue = new TypewriterQueue(charactersPerSecond: 1f);
        queue.EnqueueLine([
            new StyledSpan("Hi", LyColor.White, null, false, false, false)
        ]);

        queue.Update(TimeSpan.FromSeconds(0.4));
        Assert.That(queue.CurrentLineText, Is.EqualTo(string.Empty));

        queue.Update(TimeSpan.FromSeconds(0.6));
        Assert.That(queue.CurrentLineText, Is.EqualTo("H"));
    }
}
