using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Systems;
using NUnit.Framework;
using SadRogue.Primitives;

namespace LillyQuest.Tests.RogueLike.Systems;

public class FovSystemTests
{
    [Test]
    public void GetVisibilityFalloff_LastThreeRings_ReturnsDecreasingFactors()
    {
        var map = new LyQuestMap(20, 20);
        var fovSystem = new FovSystem(fovRadius: 5);
        fovSystem.RegisterMap(map);

        map.SetTerrain(new TerrainGameObject(new Point(5, 5), isWalkable: true, isTransparent: true));
        fovSystem.UpdateFov(map, new Point(5, 5));

        var inner = fovSystem.GetVisibilityFalloff(map, new Point(5, 8));
        var mid = fovSystem.GetVisibilityFalloff(map, new Point(5, 9));
        var edge = fovSystem.GetVisibilityFalloff(map, new Point(5, 10));

        Assert.That(inner, Is.LessThan(1f));
        Assert.That(mid, Is.LessThan(inner));
        Assert.That(edge, Is.LessThan(mid));
    }

    [Test]
    public void GetVisibilityFalloff_InsideCore_ReturnsOne()
    {
        var map = new LyQuestMap(20, 20);
        var fovSystem = new FovSystem(fovRadius: 5);
        fovSystem.RegisterMap(map);

        map.SetTerrain(new TerrainGameObject(new Point(5, 5), isWalkable: true, isTransparent: true));
        fovSystem.UpdateFov(map, new Point(5, 5));

        var core = fovSystem.GetVisibilityFalloff(map, new Point(5, 7));

        Assert.That(core, Is.EqualTo(1f));
    }
}
