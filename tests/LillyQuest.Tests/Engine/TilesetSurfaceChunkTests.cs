using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Tests.Engine;

public class TilesetSurfaceChunkTests
{
    [Test]
    public void SetTile_WithEmptyTile_DoesNotCreateChunk()
    {
        var surface = new TilesetSurface(64, 64);
        surface.Initialize(1);
        var empty = new TileRenderData(-1, LyColor.White);

        surface.SetTile(0, 10, 10, empty);

        Assert.That(surface.Layers[0].GetChunkCount(), Is.EqualTo(0));
    }
}
