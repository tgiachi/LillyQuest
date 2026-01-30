using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Tests.Game.Scenes;

public class RogueSceneFogOfWarTests
{
    [Test]
    public void LyColor_Darken_DarkensColorByFactor()
    {
        var original = new LyColor(255, 100, 150, 200);

        var darkened = original.Darken(0.5f);

        Assert.That(darkened.A, Is.EqualTo(255), "Alpha should be preserved");
        Assert.That(darkened.R, Is.EqualTo(50));
        Assert.That(darkened.G, Is.EqualTo(75));
        Assert.That(darkened.B, Is.EqualTo(100));
    }

    [Test]
    public void LyColor_Darken_WithOneFactor_ReturnsOriginal()
    {
        var original = new LyColor(255, 100, 150, 200);

        var darkened = original.Darken(1f);

        Assert.That(darkened, Is.EqualTo(original));
    }

    [Test]
    public void LyColor_Darken_WithZeroFactor_ReturnsBlack()
    {
        var original = new LyColor(255, 100, 150, 200);

        var darkened = original.Darken(0f);

        Assert.That(darkened.A, Is.EqualTo(255));
        Assert.That(darkened.R, Is.EqualTo(0));
        Assert.That(darkened.G, Is.EqualTo(0));
        Assert.That(darkened.B, Is.EqualTo(0));
    }

    [Test]
    public void TileRenderData_Darken_DarkensForegroundAndBackground()
    {
        var originalForeground = new LyColor(255, 100, 150, 200);
        var originalBackground = new LyColor(255, 50, 100, 150);
        var tile = new TileRenderData(1, originalForeground, originalBackground);

        Assert.That(tile.ForegroundColor, Is.EqualTo(originalForeground));
        Assert.That(tile.BackgroundColor, Is.EqualTo(originalBackground));

        var result = tile.Darken(0.5f);

        Assert.That(result.ForegroundColor, Is.EqualTo(new LyColor(255, 50, 75, 100)));
        Assert.That(result.BackgroundColor, Is.EqualTo(new LyColor(255, 25, 50, 75)));
    }
}
