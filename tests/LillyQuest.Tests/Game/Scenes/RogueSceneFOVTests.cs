using System.Linq;
using SadRogue.Primitives;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Services;

namespace LillyQuest.Tests.Game.Scenes;

public class RogueSceneFOVTests
{
    private LyQuestMap CreateTestMap(int width, int height)
    {
        var map = new LyQuestMap(width, height);

        // Make all positions walkable
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var terrain = new TerrainGameObject(new Point(x, y), isWalkable: true, isTransparent: true);
                map.SetTerrain(terrain);
            }
        }

        return map;
    }

    [Test]
    public void PlayerMovement_UpdatesFOV()
    {
        // Arrange
        var map = CreateTestMap(100, 100);
        var fovService = new FOVService(map);
        var startPos = new Point(10, 10);
        var endPos = new Point(60, 60);

        // Act
        fovService.UpdateFOV(startPos);
        var firstFOVTiles = new HashSet<Point>(fovService.CurrentVisibleTiles);

        fovService.UpdateFOV(endPos);
        var secondFOVTiles = new HashSet<Point>(fovService.CurrentVisibleTiles);

        // Assert - FOV should change when player moves far away
        // Check that the two FOV sets are different by looking for at least one tile in difference
        bool hasDifference = !firstFOVTiles.SetEquals(secondFOVTiles);
        Assert.That(hasDifference, Is.True, "FOV should change when player moves");
        Assert.That(fovService.CurrentVisibleTiles, Does.Contain(endPos), "Player position should be visible");
    }

    [Test]
    public void FogOfWar_ExploredTilesPersist()
    {
        // Arrange
        var map = CreateTestMap(50, 50);
        var fovService = new FOVService(map);
        var pos1 = new Point(10, 10);
        var pos2 = new Point(40, 40);

        // Act
        fovService.UpdateFOV(pos1);
        var firstExplored = fovService.ExploredTiles.Count;

        fovService.UpdateFOV(pos2); // Move far away

        // Assert - previously explored tiles should still be marked
        Assert.That(fovService.IsExplored(pos1), Is.True); // Should still be explored
        Assert.That(fovService.ExploredTiles.Count, Is.GreaterThanOrEqualTo(firstExplored)); // Can only grow or stay same
    }
}
