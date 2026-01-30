using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Systems;

namespace LillyQuest.Tests.RogueLike.Systems;

public class FovSystemTests
{
    [Test]
    public void GetVisibilityFalloff_InsideCore_ReturnsOne()
    {
        var map = new LyQuestMap(20, 20);
        var fovSystem = new FovSystem(5);
        fovSystem.RegisterMap(map);

        map.SetTerrain(new TerrainGameObject(new(5, 5)));
        fovSystem.UpdateFov(map, new(5, 5));

        var core = fovSystem.GetVisibilityFalloff(map, new(5, 7));

        Assert.That(core, Is.EqualTo(1f));
    }

    [Test]
    public void GetVisibilityFalloff_LastThreeRings_ReturnsDecreasingFactors()
    {
        var map = new LyQuestMap(20, 20);
        var fovSystem = new FovSystem(5);
        fovSystem.RegisterMap(map);

        map.SetTerrain(new TerrainGameObject(new(5, 5)));
        fovSystem.UpdateFov(map, new(5, 5));

        var inner = fovSystem.GetVisibilityFalloff(map, new(5, 8));
        var mid = fovSystem.GetVisibilityFalloff(map, new(5, 9));
        var edge = fovSystem.GetVisibilityFalloff(map, new(5, 10));

        Assert.That(inner, Is.LessThan(1f));
        Assert.That(mid, Is.LessThan(inner));
        Assert.That(edge, Is.LessThan(mid));
    }

    [Test]
    public void OnCurrentMapChanged_RegistersNewMap_AndUnregistersOldMap()
    {
        var system = new FovSystem();
        var oldMap = new LyQuestMap(10, 10);
        var newMap = new LyQuestMap(10, 10);

        system.OnCurrentMapChanged(null, oldMap);
        Assert.That(system.IsExplored(oldMap, new(0, 0)), Is.False);

        system.OnCurrentMapChanged(oldMap, newMap);

        Assert.That(system.IsExplored(newMap, new(0, 0)), Is.False);
    }

    [Test]
    public void OnMapRegistered_RegistersMap()
    {
        var map = new LyQuestMap(10, 10);
        var fovSystem = new FovSystem();

        fovSystem.OnMapRegistered(map);

        Assert.That(fovSystem.IsExplored(map, new(0, 0)), Is.False);
    }
}
