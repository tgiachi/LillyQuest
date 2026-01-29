using System.Numerics;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Services;
using LillyQuest.RogueLike.Systems;
using SadRogue.Primitives;

namespace LillyQuest.Tests.RogueLike.Services;

public class GoRogueFOVProviderTests
{
    [Test]
    public void IsVisible_WithCoordinates_ReturnsTrueForVisibleTile()
    {
        // Arrange
        var map = new LyQuestMap(20, 20);
        var fovSystem = new FovSystem();
        fovSystem.RegisterMap(map);
        
        // Add some terrain
        var floor = new TerrainGameObject(new Point(5, 5), isWalkable: true, isTransparent: true);
        map.SetTerrain(floor);
        
        // Calculate FOV from position 5,5
        fovSystem.UpdateFov(map, new Point(5, 5));
        
        var provider = new GoRogueFOVProvider(fovSystem, map);

        // Act
        var result = provider.IsVisible(5, 5);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsVisible_WithCoordinates_ReturnsFalseForHiddenTile()
    {
        // Arrange
        var map = new LyQuestMap(20, 20);
        var fovSystem = new FovSystem(fovRadius: 5); // Small radius
        fovSystem.RegisterMap(map);
        
        // Add terrain
        var floor = new TerrainGameObject(new Point(5, 5), isWalkable: true, isTransparent: true);
        map.SetTerrain(floor);
        
        // Calculate FOV from position 5,5 with radius 5 - tile at 15,15 won't be visible
        fovSystem.UpdateFov(map, new Point(5, 5));
        
        var provider = new GoRogueFOVProvider(fovSystem, map);

        // Act
        var result = provider.IsVisible(15, 15);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsVisible_WithVector2_ReturnsTrueForVisibleTile()
    {
        // Arrange
        var map = new LyQuestMap(20, 20);
        var fovSystem = new FovSystem();
        fovSystem.RegisterMap(map);
        
        var floor = new TerrainGameObject(new Point(10, 10), isWalkable: true, isTransparent: true);
        map.SetTerrain(floor);
        
        fovSystem.UpdateFov(map, new Point(10, 10));
        
        var provider = new GoRogueFOVProvider(fovSystem, map);

        // Act
        var result = provider.IsVisible(new Vector2(10.5f, 10.3f));

        // Assert
        Assert.That(result, Is.True);
    }
}
