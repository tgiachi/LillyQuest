using System;
using System.Linq;
using NUnit.Framework;
using SadRogue.Primitives;
using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Data.Tiles;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Maps.Tiles;
using LillyQuest.RogueLike.Systems;

namespace LillyQuest.Tests.RogueLike.Systems;

public class FovSystemTests
{
    private LyQuestMap CreateTestMap(int width = 50, int height = 50)
    {
        var map = new LyQuestMap(width, height);

        // Make all positions walkable and transparent for testing by adding terrain
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var terrain = new TerrainGameObject(
                    new Point(x, y),
                    isWalkable: true,
                    isTransparent: true
                )
                {
                    Tile = new VisualTile("test_floor", ".", LyColor.Black, LyColor.White)
                };

                map.SetTerrain(terrain);
            }
        }

        return map;
    }

    [Test]
    public void UpdateFov_WithValidPosition_CalculatesVisibleTiles()
    {
        // Arrange
        var map = CreateTestMap();
        var system = new FovSystem();
        system.RegisterMap(map);
        var playerPos = new Point(25, 25);

        // Act
        system.UpdateFov(map, playerPos);

        // Assert
        var visibleTiles = system.GetCurrentVisibleTiles(map);
        Assert.That(visibleTiles, Is.Not.Empty);
        Assert.That(visibleTiles, Does.Contain(playerPos)); // Player always sees own position
    }

    [Test]
    public void UpdateFov_WithInvalidPosition_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var map = CreateTestMap(50, 50);
        var system = new FovSystem();
        system.RegisterMap(map);
        var invalidPos = new Point(100, 100); // Outside bounds

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => system.UpdateFov(map, invalidPos));
    }

    [Test]
    public void IsVisible_WithVisiblePosition_ReturnsTrue()
    {
        // Arrange
        var map = CreateTestMap();
        var system = new FovSystem();
        system.RegisterMap(map);
        var playerPos = new Point(25, 25);
        system.UpdateFov(map, playerPos);

        // Act
        var isVisible = system.IsVisible(map, playerPos);

        // Assert
        Assert.That(isVisible, Is.True);
    }

    [Test]
    public void IsExplored_AfterUpdateFov_MarksTilesAsExplored()
    {
        // Arrange
        var map = CreateTestMap();
        var system = new FovSystem();
        system.RegisterMap(map);
        var playerPos = new Point(25, 25);

        // Act
        system.UpdateFov(map, playerPos);

        // Assert
        foreach (var visiblePos in system.GetCurrentVisibleTiles(map))
        {
            Assert.That(system.IsExplored(map, visiblePos), Is.True);
        }
    }

    [Test]
    public void UpdateFov_SamePositionTwice_DoesNotRecalculate()
    {
        // Arrange
        var map = CreateTestMap();
        var system = new FovSystem();
        system.RegisterMap(map);
        var playerPos = new Point(25, 25);

        // Act
        system.UpdateFov(map, playerPos);
        var firstSet = system.GetCurrentVisibleTiles(map).ToHashSet();

        system.UpdateFov(map, playerPos); // Same position
        var secondSet = system.GetCurrentVisibleTiles(map).ToHashSet();

        // Assert - sets should be identical (no recalculation = cached result)
        Assert.That(secondSet, Is.EqualTo(firstSet));
    }

    [Test]
    public void MemorizeTile_StoresTileData_AndCanBeRetrieved()
    {
        // Arrange
        var map = CreateTestMap();
        var system = new FovSystem();
        system.RegisterMap(map);
        var pos = new Point(10, 10);
        var memory = new TileMemory('@', Color.White, Color.Black);

        // Act
        system.MemorizeTile(map, pos, memory.Symbol, memory.ForegroundColor, memory.BackgroundColor);
        var retrieved = system.GetMemorizedTile(map, pos);

        // Assert
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved, Is.EqualTo(memory));
    }

    [Test]
    public void UnregisterMap_RemovesMapState()
    {
        // Arrange
        var map = CreateTestMap();
        var system = new FovSystem();
        system.RegisterMap(map);
        system.UpdateFov(map, new Point(25, 25));

        // Act
        system.UnregisterMap(map);

        // Assert
        Assert.That(system.GetCurrentVisibleTiles(map), Is.Empty);
        Assert.That(system.IsVisible(map, new Point(25, 25)), Is.False);
    }
}
