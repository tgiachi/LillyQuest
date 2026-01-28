using LillyQuest.Game.Rendering;
using NUnit.Framework;

namespace LillyQuest.Tests.Game.Rendering;

public class DirtyChunkTrackerTests
{
    [Test]
    public void GetChunkCoord_UsesChunkSize()
    {
        var tracker = new DirtyChunkTracker(chunkSize: 32);

        Assert.That(tracker.GetChunkCoord(0, 0), Is.EqualTo(new ChunkCoord(0, 0)));
        Assert.That(tracker.GetChunkCoord(31, 31), Is.EqualTo(new ChunkCoord(0, 0)));
        Assert.That(tracker.GetChunkCoord(32, 0), Is.EqualTo(new ChunkCoord(1, 0)));
        Assert.That(tracker.GetChunkCoord(0, 32), Is.EqualTo(new ChunkCoord(0, 1)));
    }
}
