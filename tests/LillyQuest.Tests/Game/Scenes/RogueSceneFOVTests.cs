using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Systems;
using SadRogue.Primitives;

namespace LillyQuest.Tests.Game.Scenes;

public class RogueSceneFOVTests
{
    [Test]
    public void FogOfWar_ExploredTilesPersist()
    {
        // Arrange
        var map = CreateTestMap(50, 50);
        var fovSystem = new FovSystem();
        fovSystem.RegisterMap(map);
        var pos1 = new Point(10, 10);
        var pos2 = new Point(40, 40);

        // Act
        fovSystem.UpdateFov(map, pos1);
        var firstExplored = fovSystem.GetExploredTiles(map).Count;

        fovSystem.UpdateFov(map, pos2); // Move far away

        // Assert - previously explored tiles should still be marked
        Assert.That(fovSystem.IsExplored(map, pos1), Is.True); // Should still be explored
        Assert.That(
            fovSystem.GetExploredTiles(map).Count,
            Is.GreaterThanOrEqualTo(firstExplored)
        ); // Can only grow or stay same
    }

    [Test]
    public void PlayerMovement_UpdatesFOV()
    {
        // Arrange
        var map = CreateTestMap(100, 100);
        var fovSystem = new FovSystem();
        fovSystem.RegisterMap(map);
        var startPos = new Point(10, 10);
        var endPos = new Point(60, 60);

        // Act
        fovSystem.UpdateFov(map, startPos);
        var firstFOVTiles = new HashSet<Point>(fovSystem.GetCurrentVisibleTiles(map));

        fovSystem.UpdateFov(map, endPos);
        var secondFOVTiles = new HashSet<Point>(fovSystem.GetCurrentVisibleTiles(map));

        // Assert - FOV should change when player moves far away
        // Check that the two FOV sets are different by looking for at least one tile in difference
        var hasDifference = !firstFOVTiles.SetEquals(secondFOVTiles);
        Assert.That(hasDifference, Is.True, "FOV should change when player moves");
        Assert.That(fovSystem.GetCurrentVisibleTiles(map), Does.Contain(endPos), "Player position should be visible");
    }

    private LyQuestMap CreateTestMap(int width, int height)
    {
        var map = new LyQuestMap(width, height);

        // Make all positions walkable
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var terrain = new TerrainGameObject(new(x, y));
                map.SetTerrain(terrain);
            }
        }

        return map;
    }
}
