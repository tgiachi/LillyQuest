using System.Numerics;
using LillyQuest.Engine.Interfaces.Particles;

namespace LillyQuest.Tests.Engine;

public class ParticleFOVProviderTests
{
    private sealed class FakeFOVProvider : IParticleFOVProvider
    {
        private readonly Dictionary<(int X, int Y), bool> _visibleTiles = new();

        public bool IsVisible(int x, int y)
            => _visibleTiles.TryGetValue((x, y), out var visible) && visible;

        public bool IsVisible(Vector2 worldPosition)
        {
            var x = (int)worldPosition.X;
            var y = (int)worldPosition.Y;

            return IsVisible(x, y);
        }

        public void SetVisible(int x, int y, bool isVisible)
        {
            _visibleTiles[(x, y)] = isVisible;
        }
    }

    [Test]
    public void IsVisible_WithCoordinates_ReturnsFalseForHiddenTile()
    {
        // Arrange
        var provider = new FakeFOVProvider();
        provider.SetVisible(5, 5, false);

        // Act
        var result = provider.IsVisible(5, 5);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsVisible_WithCoordinates_ReturnsTrueForVisibleTile()
    {
        // Arrange
        var provider = new FakeFOVProvider();
        provider.SetVisible(5, 5, true);

        // Act
        var result = provider.IsVisible(5, 5);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsVisible_WithVector2_ReturnsFalseForHiddenTile()
    {
        // Arrange
        var provider = new FakeFOVProvider();
        provider.SetVisible(10, 20, false);

        // Act
        var result = provider.IsVisible(new(10.5f, 20.3f));

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsVisible_WithVector2_ReturnsTrueForVisibleTile()
    {
        // Arrange
        var provider = new FakeFOVProvider();
        provider.SetVisible(10, 20, true);

        // Act
        var result = provider.IsVisible(new(10.5f, 20.3f));

        // Assert
        Assert.That(result, Is.True);
    }
}
