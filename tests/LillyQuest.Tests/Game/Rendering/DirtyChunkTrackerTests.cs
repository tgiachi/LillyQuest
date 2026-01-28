using LillyQuest.RogueLike.Rendering;
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

    [Test]
    public void MarkDirtyForTile_AddsChunk()
    {
        var tracker = new DirtyChunkTracker(chunkSize: 16);

        tracker.MarkDirtyForTile(17, 0);

        Assert.That(tracker.DirtyChunks, Does.Contain(new ChunkCoord(1, 0)));
    }
}
