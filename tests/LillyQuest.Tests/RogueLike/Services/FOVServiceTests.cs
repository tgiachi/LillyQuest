using System;
using System.Linq;
using NUnit.Framework;
using SadRogue.Primitives;
using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Maps.Tiles;
using LillyQuest.RogueLike.Services;

namespace LillyQuest.Tests.RogueLike.Services;

public class FOVServiceTests
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
    public void UpdateFOV_WithValidPosition_CalculatesVisibleTiles()
    {
        // Arrange
        var map = CreateTestMap();
        var service = new FOVService(map);
        var playerPos = new Point(25, 25);

        // Act
        service.UpdateFOV(playerPos);

        // Assert
        Assert.That(service.CurrentVisibleTiles, Is.Not.Empty);
        Assert.That(service.CurrentVisibleTiles, Does.Contain(playerPos)); // Player always sees own position
    }

    [Test]
    public void UpdateFOV_WithInvalidPosition_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var map = CreateTestMap(50, 50);
        var service = new FOVService(map);
        var invalidPos = new Point(100, 100); // Outside bounds

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => service.UpdateFOV(invalidPos));
    }

    [Test]
    public void IsVisible_WithVisiblePosition_ReturnsTrue()
    {
        // Arrange
        var map = CreateTestMap();
        var service = new FOVService(map);
        var playerPos = new Point(25, 25);
        service.UpdateFOV(playerPos);

        // Act
        var isVisible = service.IsVisible(playerPos);

        // Assert
        Assert.That(isVisible, Is.True);
    }

    [Test]
    public void IsExplored_AfterUpdateFOV_MarksTilesAsExplored()
    {
        // Arrange
        var map = CreateTestMap();
        var service = new FOVService(map);
        var playerPos = new Point(25, 25);

        // Act
        service.UpdateFOV(playerPos);

        // Assert
        foreach (var visiblePos in service.CurrentVisibleTiles)
        {
            Assert.That(service.IsExplored(visiblePos), Is.True);
        }
    }

    [Test]
    public void UpdateFOV_SamePositionTwice_DoesNotRecalculate()
    {
        // Arrange
        var map = CreateTestMap();
        var service = new FOVService(map);
        var playerPos = new Point(25, 25);

        // Act
        service.UpdateFOV(playerPos);
        var firstSet = service.CurrentVisibleTiles.ToHashSet();

        service.UpdateFOV(playerPos); // Same position
        var secondSet = service.CurrentVisibleTiles.ToHashSet();

        // Assert - sets should be identical (no recalculation = cached result)
        Assert.That(secondSet, Is.EqualTo(firstSet));
    }

    [Test]
    public void MemorializeTile_StoresTileData_AndCanBeRetrieved()
    {
        // Arrange
        var map = CreateTestMap();
        var service = new FOVService(map);
        var pos = new Point(10, 10);
        var memory = new TileMemory('@', SadRogue.Primitives.Color.White, SadRogue.Primitives.Color.Black);

        // Act
        service.MemorializeTile(pos, memory.Symbol, memory.ForegroundColor, memory.BackgroundColor);
        var retrieved = service.GetMemorizedTile(pos);

        // Assert
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved, Is.EqualTo(memory));
    }
}
