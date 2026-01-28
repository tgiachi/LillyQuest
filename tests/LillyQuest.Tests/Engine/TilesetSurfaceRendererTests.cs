using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Screens.TilesetSurface;
using NUnit.Framework;
using System.Numerics;

namespace LillyQuest.Tests.Engine;

public class TilesetSurfaceRendererTests
{
    [Test]
    public void CalculateVisibleTileRange_ReturnsCorrectBounds()
    {
        var result = TilesetSurfaceRenderer.CalculateVisibleTileRange(
            visibleX: 0f,
            visibleY: 0f,
            visibleWidth: 160f,
            visibleHeight: 160f,
            scaledTileWidth: 16f,
            scaledTileHeight: 16f,
            surfaceWidth: 100,
            surfaceHeight: 100
        );

        Assert.That(result.minTileX, Is.EqualTo(0));
        Assert.That(result.minTileY, Is.EqualTo(0));
        Assert.That(result.maxTileX, Is.EqualTo(11));
        Assert.That(result.maxTileY, Is.EqualTo(11));
    }

    [Test]
    public void CalculateVisibleTileRange_ClampsToSurfaceBounds()
    {
        var result = TilesetSurfaceRenderer.CalculateVisibleTileRange(
            visibleX: -100f,  // Starts before the surface
            visibleY: -100f,
            visibleWidth: 1000f,  // Extends beyond the surface
            visibleHeight: 1000f,
            scaledTileWidth: 16f,
            scaledTileHeight: 16f,
            surfaceWidth: 10,
            surfaceHeight: 10
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
            visibleX: 32f,  // Start at tile 2
            visibleY: 48f,  // Start at tile 3
            visibleWidth: 64f,
            visibleHeight: 64f,
            scaledTileWidth: 16f,
            scaledTileHeight: 16f,
            surfaceWidth: 100,
            surfaceHeight: 100
        );

        Assert.That(result.minTileX, Is.EqualTo(2));
        Assert.That(result.minTileY, Is.EqualTo(3));
        Assert.That(result.maxTileX, Is.EqualTo(7));
        Assert.That(result.maxTileY, Is.EqualTo(8));
    }
}
