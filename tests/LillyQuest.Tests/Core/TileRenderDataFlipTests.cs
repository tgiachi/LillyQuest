using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Types;
using Silk.NET.Maths;

namespace LillyQuest.Tests.Core;

public class TileRenderDataFlipTests
{
    [Test]
    public void ApplyFlip_None_ReturnsOriginal()
    {
        var rect = new Rectangle<float>(0.1f, 0.2f, 0.3f, 0.4f);

        var result = TileRenderData.ApplyFlip(rect, TileFlipType.None);

        Assert.That(result.Origin.X, Is.EqualTo(0.1f));
        Assert.That(result.Origin.Y, Is.EqualTo(0.2f));
        Assert.That(result.Size.X, Is.EqualTo(0.3f));
        Assert.That(result.Size.Y, Is.EqualTo(0.4f));
    }

    [Test]
    public void ApplyFlip_Horizontal_FlipsU()
    {
        var rect = new Rectangle<float>(0.1f, 0.2f, 0.3f, 0.4f);

        var result = TileRenderData.ApplyFlip(rect, TileFlipType.FlipHorizontal);

        Assert.That(result.Origin.X, Is.EqualTo(0.4f));
        Assert.That(result.Origin.Y, Is.EqualTo(0.2f));
        Assert.That(result.Size.X, Is.EqualTo(-0.3f));
        Assert.That(result.Size.Y, Is.EqualTo(0.4f));
    }

    [Test]
    public void ApplyFlip_Vertical_FlipsV()
    {
        var rect = new Rectangle<float>(0.1f, 0.2f, 0.3f, 0.4f);

        var result = TileRenderData.ApplyFlip(rect, TileFlipType.FlipVertical);

        Assert.That(result.Origin.X, Is.EqualTo(0.1f));
        Assert.That(result.Origin.Y, Is.EqualTo(0.6f));
        Assert.That(result.Size.X, Is.EqualTo(0.3f));
        Assert.That(result.Size.Y, Is.EqualTo(-0.4f));
    }

    [Test]
    public void ApplyFlip_Both_FlipsUV()
    {
        var rect = new Rectangle<float>(0.1f, 0.2f, 0.3f, 0.4f);

        var result = TileRenderData.ApplyFlip(rect, TileFlipType.FlipHorizontal | TileFlipType.FlipVertical);

        Assert.That(result.Origin.X, Is.EqualTo(0.4f));
        Assert.That(result.Origin.Y, Is.EqualTo(0.6f));
        Assert.That(result.Size.X, Is.EqualTo(-0.3f));
        Assert.That(result.Size.Y, Is.EqualTo(-0.4f));
    }
}
