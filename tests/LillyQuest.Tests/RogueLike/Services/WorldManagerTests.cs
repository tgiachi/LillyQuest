using LillyQuest.Engine.Interfaces.Services;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Services;
using NSubstitute;
using NUnit.Framework;

namespace LillyQuest.Tests.RogueLike.Services;

public class WorldManagerTests
{
    [Test]
    public void SetCurrentMap_NotifiesHandlersInOrder()
    {
        var mapGenerator = Substitute.For<IMapGenerator>();
        var jobScheduler = Substitute.For<IJobScheduler>();
        var worldManager = new WorldManager(mapGenerator, jobScheduler);
        var handler = new FakeMapHandler();

        worldManager.RegisterMapHandler(handler);

        var oldMap = new LyQuestMap(10, 10);
        var newMap = new LyQuestMap(10, 10);

        worldManager.CurrentMap = oldMap;
        handler.Reset();
        worldManager.CurrentMap = newMap;

        Assert.That(
            handler.Calls,
            Is.EqualTo(new[] { "unregister", "register", "change" })
        );
    }

    [Test]
    public void UnregisterMapHandler_StopsNotifications()
    {
        var mapGenerator = Substitute.For<IMapGenerator>();
        var jobScheduler = Substitute.For<IJobScheduler>();
        var worldManager = new WorldManager(mapGenerator, jobScheduler);
        var handler = new FakeMapHandler();

        worldManager.RegisterMapHandler(handler);
        worldManager.UnregisterMapHandler(handler);

        worldManager.CurrentMap = new LyQuestMap(10, 10);

        Assert.That(handler.Calls, Is.Empty);
    }

    private sealed class FakeMapHandler : IMapHandler
    {
        public List<string> Calls { get; } = new();

        public void Reset() => Calls.Clear();

        public void OnMapRegistered(LyQuestMap map) => Calls.Add("register");
        public void OnMapUnregistered(LyQuestMap map) => Calls.Add("unregister");
        public void OnCurrentMapChanged(LyQuestMap? oldMap, LyQuestMap newMap) => Calls.Add("change");
    }
}
