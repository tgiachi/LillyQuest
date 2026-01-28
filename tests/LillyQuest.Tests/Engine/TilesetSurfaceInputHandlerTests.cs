using LillyQuest.Engine.Screens.TilesetSurface;
using NUnit.Framework;
using System.Numerics;

namespace LillyQuest.Tests.Engine;

public class TilesetSurfaceInputHandlerTests
{
    [Test]
    public void GetInputTileCoordinates_ReturnsCorrectTile()
    {
        var context = new TilesetSurfaceInputContext
        {
            TileRenderScale = 1.0f,
            ScreenPosition = Vector2.Zero,
            Margin = Vector4.Zero
        };

        var handler = new TilesetSurfaceInputHandler(context);

        // Layer with 16x16 tiles, scale 1.0, no offset
        var result = handler.GetInputTileCoordinates(
            layerRenderScale: 1.0f,
            tileWidth: 16,
            tileHeight: 16,
            layerPixelOffset: Vector2.Zero,
            viewTileOffset: Vector2.Zero,
            viewPixelOffset: Vector2.Zero,
            mouseX: 32,
            mouseY: 48
        );

        Assert.That(result.x, Is.EqualTo(2));
        Assert.That(result.y, Is.EqualTo(3));
    }

    [Test]
    public void GetInputTileCoordinates_AccountsForViewOffset()
    {
        var context = new TilesetSurfaceInputContext
        {
            TileRenderScale = 1.0f,
            ScreenPosition = Vector2.Zero,
            Margin = Vector4.Zero
        };

        var handler = new TilesetSurfaceInputHandler(context);

        var result = handler.GetInputTileCoordinates(
            layerRenderScale: 1.0f,
            tileWidth: 16,
            tileHeight: 16,
            layerPixelOffset: Vector2.Zero,
            viewTileOffset: new Vector2(2, 3),  // Scrolled 2 tiles right, 3 down
            viewPixelOffset: Vector2.Zero,
            mouseX: 0,
            mouseY: 0
        );

        Assert.That(result.x, Is.EqualTo(2));
        Assert.That(result.y, Is.EqualTo(3));
    }

    [Test]
    public void HitTest_ReturnsTrue_WhenInsideBounds()
    {
        var context = new TilesetSurfaceInputContext
        {
            TileRenderScale = 1.0f,
            ScreenPosition = new Vector2(100, 100),
            Margin = Vector4.Zero
        };

        var handler = new TilesetSurfaceInputHandler(context);

        Assert.That(handler.HitTest(150, 150, new Vector2(200, 200)), Is.True);
        Assert.That(handler.HitTest(50, 50, new Vector2(200, 200)), Is.False);
    }
}
