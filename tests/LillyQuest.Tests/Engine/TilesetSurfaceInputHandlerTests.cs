using System.Numerics;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Tests.Engine;

public class TilesetSurfaceInputHandlerTests
{
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
            1.0f,
            16,
            16,
            Vector2.Zero,
            new(2, 3), // Scrolled 2 tiles right, 3 down
            Vector2.Zero,
            0,
            0
        );

        Assert.That(result.x, Is.EqualTo(2));
        Assert.That(result.y, Is.EqualTo(3));
    }

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
            1.0f,
            16,
            16,
            Vector2.Zero,
            Vector2.Zero,
            Vector2.Zero,
            32,
            48
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
            ScreenPosition = new(100, 100),
            Margin = Vector4.Zero
        };

        var handler = new TilesetSurfaceInputHandler(context);

        Assert.That(handler.HitTest(150, 150, new(200, 200)), Is.True);
        Assert.That(handler.HitTest(50, 50, new(200, 200)), Is.False);
    }
}
