using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Tests.Engine;

public class TilesetSurfaceRendererTests
{
    [Test]
    public void CalculateVisibleTileRange_ClampsToSurfaceBounds()
    {
        var result = TilesetSurfaceRenderer.CalculateVisibleTileRange(
            -100f, // Starts before the surface
            -100f,
            1000f, // Extends beyond the surface
            1000f,
            16f,
            16f,
            10,
            10
        );

        Assert.That(result.minTileX, Is.EqualTo(0));
        Assert.That(result.minTileY, Is.EqualTo(0));
        Assert.That(result.maxTileX, Is.EqualTo(10));
        Assert.That(result.maxTileY, Is.EqualTo(10));
    }

    [Test]
    public void CalculateVisibleTileRange_HandlesOffset()
    {
        var result = TilesetSurfaceRenderer.CalculateVisibleTileRange(
            32f, // Start at tile 2
            48f, // Start at tile 3
            64f,
            64f,
            16f,
            16f,
            100,
            100
        );

        Assert.That(result.minTileX, Is.EqualTo(2));
        Assert.That(result.minTileY, Is.EqualTo(3));
        Assert.That(result.maxTileX, Is.EqualTo(7));
        Assert.That(result.maxTileY, Is.EqualTo(8));
    }

    [Test]
    public void CalculateVisibleTileRange_ReturnsCorrectBounds()
    {
        var result = TilesetSurfaceRenderer.CalculateVisibleTileRange(
            0f,
            0f,
            160f,
            160f,
            16f,
            16f,
            100,
            100
        );

        Assert.That(result.minTileX, Is.EqualTo(0));
        Assert.That(result.minTileY, Is.EqualTo(0));
        Assert.That(result.maxTileX, Is.EqualTo(11));
        Assert.That(result.maxTileY, Is.EqualTo(11));
    }
}
