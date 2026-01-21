using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Tests;

public class TilesetSurfaceStorageTests
{
    [Test]
    public void SetTile_WritesAndGetTile_ReadsFromLayer()
    {
        var surface = new TilesetSurface(2, 2);
        surface.Initialize(1);
        var tile = new TileRenderData(7, LyColor.White);

        surface.SetTile(0, 1, 1, tile);

        Assert.That(surface.GetTile(0, 1, 1).TileIndex, Is.EqualTo(7));
    }
}
