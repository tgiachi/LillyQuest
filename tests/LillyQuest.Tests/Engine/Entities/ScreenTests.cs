using System.Numerics;
using LillyQuest.Engine.Entities;
using NUnit.Framework;

namespace LillyQuest.Tests.Engine.Entities;

public class ScreenTests
{
    [Test]
    public void WorldToLocal_TransformsCorrectly()
    {
        // Arrange
        var screen = new Screen(1, "TestScreen")
        {
            Position = new Vector2(100, 200)
        };
        var worldPos = new Vector2(150, 250);

        // Act
        var localPos = screen.WorldToLocal(worldPos);

        // Assert
        Assert.That(localPos, Is.EqualTo(new Vector2(50, 50)));
    }

    [Test]
    public void LocalToWorld_TransformsCorrectly()
    {
        // Arrange
        var screen = new Screen(1, "TestScreen")
        {
            Position = new Vector2(100, 200)
        };
        var localPos = new Vector2(50, 50);

        // Act
        var worldPos = screen.LocalToWorld(localPos);

        // Assert
        Assert.That(worldPos, Is.EqualTo(new Vector2(150, 250)));
    }

    [Test]
    public void ContainsPoint_InsideScreen_ReturnsTrue()
    {
        // Arrange
        var screen = new Screen(1, "TestScreen")
        {
            Position = new Vector2(100, 100),
            Size = new Vector2(200, 200)
        };
        var point = new Vector2(150, 150);

        // Act
        var contains = screen.ContainsPoint(point);

        // Assert
        Assert.That(contains, Is.True);
    }

    [Test]
    public void ContainsPoint_OutsideScreen_ReturnsFalse()
    {
        // Arrange
        var screen = new Screen(1, "TestScreen")
        {
            Position = new Vector2(100, 100),
            Size = new Vector2(200, 200)
        };
        var point = new Vector2(50, 50);

        // Act
        var contains = screen.ContainsPoint(point);

        // Assert
        Assert.That(contains, Is.False);
    }

    [Test]
    public void ContainsPoint_OnEdge_ReturnsTrue()
    {
        // Arrange
        var screen = new Screen(1, "TestScreen")
        {
            Position = new Vector2(100, 100),
            Size = new Vector2(200, 200)
        };
        var point = new Vector2(100, 100); // Top-left corner

        // Act
        var contains = screen.ContainsPoint(point);

        // Assert
        Assert.That(contains, Is.True);
    }

    [Test]
    public void Bounds_ReturnsCorrectRectangle()
    {
        // Arrange
        var screen = new Screen(1, "TestScreen")
        {
            Position = new Vector2(50, 75),
            Size = new Vector2(300, 400)
        };

        // Act
        var bounds = screen.Bounds;

        // Assert
        Assert.That(bounds.X, Is.EqualTo(50));
        Assert.That(bounds.Y, Is.EqualTo(75));
        Assert.That(bounds.Width, Is.EqualTo(300));
        Assert.That(bounds.Height, Is.EqualTo(400));
    }

    [Test]
    public void IsVisible_DefaultsToTrue()
    {
        // Arrange & Act
        var screen = new Screen(1, "TestScreen");

        // Assert
        Assert.That(screen.IsVisible, Is.True);
    }
}
