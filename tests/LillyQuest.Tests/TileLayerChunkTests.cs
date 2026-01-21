using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Tests;

public class TileLayerChunkTests
{
    [Test]
    public void SetTile_CreatesChunkAndStoresTile()
    {
        var layer = new TileLayer(64, 64);
        var tile = new TileRenderData(7, LyColor.White);

        layer.SetTile(33, 1, tile);

        Assert.That(layer.GetChunkCount(), Is.EqualTo(1));
        Assert.That(layer.GetTile(33, 1).TileIndex, Is.EqualTo(7));
    }
}
