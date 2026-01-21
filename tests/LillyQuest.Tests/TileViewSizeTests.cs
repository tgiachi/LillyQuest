using System.Numerics;
using LillyQuest.Game.Screens.TilesetSurface;
using NUnit.Framework;

namespace LillyQuest.Tests;

public class TileViewSizeTests
{
    [Test]
    public void ComputeScreenSizeFromTileView_UsesScaleAndTileSize()
    {
        var tileViewSize = new Vector2(90, 30);
        var result = TilesetSurfaceScreen.ComputeScreenSizeFromTileView(tileViewSize, 12, 12, 2.0f);

        Assert.That(result.X, Is.EqualTo(2160));
        Assert.That(result.Y, Is.EqualTo(720));
    }
}
