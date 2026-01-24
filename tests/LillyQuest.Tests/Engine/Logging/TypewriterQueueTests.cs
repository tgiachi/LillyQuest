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
}
