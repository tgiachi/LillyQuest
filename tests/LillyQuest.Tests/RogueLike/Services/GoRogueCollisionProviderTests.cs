using System.Numerics;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Services;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;

namespace LillyQuest.Tests.RogueLike.Services;

public class GoRogueCollisionProviderTests
{
    [Test]
    public void IsBlocked_WithCoordinates_ReturnsTrueForWall()
    {
        // Arrange
        var map = new LyQuestMap(20, 20);
        var wall = new TerrainGameObject(new Point(5, 5), isWalkable: false, isTransparent: false);
        map.SetTerrain(wall);
        
        var provider = new GoRogueCollisionProvider();
        provider.SetMap(map);

        // Act
        var result = provider.IsBlocked(5, 5);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsBlocked_WithCoordinates_ReturnsFalseForFloor()
    {
        // Arrange
        var map = new LyQuestMap(20, 20);
        var floor = new TerrainGameObject(new Point(5, 5), isWalkable: true, isTransparent: true);
        map.SetTerrain(floor);
        
        var provider = new GoRogueCollisionProvider();
        provider.SetMap(map);

        // Act
        var result = provider.IsBlocked(5, 5);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsBlocked_WithVector2_ReturnsTrueForWall()
    {
        // Arrange
        var map = new LyQuestMap(20, 20);
        var wall = new TerrainGameObject(new Point(10, 15), isWalkable: false, isTransparent: false);
        map.SetTerrain(wall);
        
        var provider = new GoRogueCollisionProvider();
        provider.SetMap(map);

        // Act
        var result = provider.IsBlocked(new Vector2(10.5f, 15.3f));

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsBlocked_EmptyTile_ReturnsFalse()
    {
        // Arrange
        var map = new LyQuestMap(20, 20);
        var provider = new GoRogueCollisionProvider();
        provider.SetMap(map);

        // Act
        var result = provider.IsBlocked(5, 5);

        // Assert
        Assert.That(result, Is.False);
    }
}
