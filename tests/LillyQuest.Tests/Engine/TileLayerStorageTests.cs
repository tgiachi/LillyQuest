using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Tests.Engine;

public class TileLayerStorageTests
{
    [Test]
    public void GetSetTile_UsesRowMajorIndexing()
    {
        var layer = new TileLayer(3, 2);
        var tileA = new TileRenderData(1, LyColor.White);
        var tileB = new TileRenderData(2, LyColor.White);

        layer.SetTile(1, 0, tileA);
        layer.SetTile(0, 1, tileB);

        Assert.That(layer.GetTile(1, 0).TileIndex, Is.EqualTo(1));
        Assert.That(layer.GetTile(0, 1).TileIndex, Is.EqualTo(2));
        Assert.That(layer.GetTile(0, 0).TileIndex, Is.EqualTo(-1));
    }
}
