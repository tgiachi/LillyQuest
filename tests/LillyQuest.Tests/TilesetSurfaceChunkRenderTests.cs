using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Tests;

public class TilesetSurfaceChunkRenderTests
{
    [Test]
    public void EnumerateChunksInRange_ReturnsOnlyChunksOverlappingRange()
    {
        var layer = new TileLayer(128, 128);
        var tile = new TileRenderData(1, LyColor.White);

        layer.SetTile(5, 5, tile);
        layer.SetTile(70, 5, tile);

        var chunks = layer.EnumerateChunksInRange(0, 0, 63, 63).ToList();

        Assert.That(chunks.Count, Is.EqualTo(1));
        Assert.That(chunks.Any(chunk => chunk.chunkX == 0 && chunk.chunkY == 0), Is.True);
    }
}
