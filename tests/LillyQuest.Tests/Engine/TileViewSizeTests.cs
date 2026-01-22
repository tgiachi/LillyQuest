using System.Numerics;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Tests.Engine;

public class TileViewSizeTests
{
    [Test]
    public void ApplyTileViewSize_UpdatesSize()
    {
        var result = TilesetSurfaceScreen.ApplyTileViewSize(
            new(90, 30),
            new(10, 10),
            12,
            12,
            2.0f
        );

        Assert.That(result.X, Is.EqualTo(2160));
        Assert.That(result.Y, Is.EqualTo(720));
    }

    [Test]
    public void ComputeScreenSizeFromTileView_UsesScaleAndTileSize()
    {
        var tileViewSize = new Vector2(90, 30);
        var result = TilesetSurfaceScreen.ComputeScreenSizeFromTileView(tileViewSize, 12, 12, 2.0f);

        Assert.That(result.X, Is.EqualTo(2160));
        Assert.That(result.Y, Is.EqualTo(720));
    }
}
