using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Tests.Engine;

public class TilesetSurfaceMouseWheelTests
{
    [Test]
    public void HandleMouseWheel_ValidTile_ReturnsDelta()
    {
        var surface = new TilesetSurface();
        surface.Initialize(1);

        var delta = surface.HandleMouseWheel(0, 0, 0, 1.5f);

        Assert.That(delta, Is.EqualTo(1.5f));
    }

    [Test]
    public void HandleMouseWheel_InvalidLayer_ReturnsZero()
    {
        var surface = new TilesetSurface();
        surface.Initialize(1);

        var delta = surface.HandleMouseWheel(1, 0, 0, 1.5f);

        Assert.That(delta, Is.EqualTo(0f));
    }

    [Test]
    public void HandleMouseWheel_OutOfBounds_ReturnsZero()
    {
        var surface = new TilesetSurface();
        surface.Initialize(1);

        var delta = surface.HandleMouseWheel(0, -1, 0, 1.5f);

        Assert.That(delta, Is.EqualTo(0f));
    }
}
