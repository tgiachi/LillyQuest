using System.Numerics;
using LillyQuest.Engine.Interfaces.Particles;

namespace LillyQuest.Tests.Engine;

public class ParticleCollisionProviderTests
{
    private class FakeCollisionProvider : IParticleCollisionProvider
    {
        private readonly Dictionary<(int X, int Y), bool> _blockedTiles = new();

        public bool IsBlocked(int x, int y)
            => _blockedTiles.TryGetValue((x, y), out var blocked) && blocked;

        public bool IsBlocked(Vector2 worldPosition)
        {
            var x = (int)worldPosition.X;
            var y = (int)worldPosition.Y;

            return IsBlocked(x, y);
        }

        public void SetBlocked(int x, int y, bool isBlocked)
        {
            _blockedTiles[(x, y)] = isBlocked;
        }
    }

    [Test]
    public void IsBlocked_WithCoordinates_ReturnsFalseForWalkableTile()
    {
        // Arrange
        var provider = new FakeCollisionProvider();
        provider.SetBlocked(5, 5, false);

        // Act
        var result = provider.IsBlocked(5, 5);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsBlocked_WithCoordinates_ReturnsTrueForBlockedTile()
    {
        // Arrange
        var provider = new FakeCollisionProvider();
        provider.SetBlocked(5, 5, true);

        // Act
        var result = provider.IsBlocked(5, 5);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsBlocked_WithVector2_ReturnsFalseForWalkableTile()
    {
        // Arrange
        var provider = new FakeCollisionProvider();
        provider.SetBlocked(10, 20, false);

        // Act
        var result = provider.IsBlocked(new(10.5f, 20.3f));

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsBlocked_WithVector2_ReturnsTrueForBlockedTile()
    {
        // Arrange
        var provider = new FakeCollisionProvider();
        provider.SetBlocked(10, 20, true);

        // Act
        var result = provider.IsBlocked(new(10.5f, 20.3f));

        // Assert
        Assert.That(result, Is.True);
    }
}
