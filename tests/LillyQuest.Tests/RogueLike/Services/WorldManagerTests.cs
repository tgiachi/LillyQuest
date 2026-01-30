using LillyQuest.Engine.Interfaces.Services;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Services;
using NSubstitute;

namespace LillyQuest.Tests.RogueLike.Services;

public class WorldManagerTests
{
    private sealed class ImmediateJobScheduler : IJobScheduler
    {
        public void Enqueue(Action job)
            => job();

        public void Enqueue(Func<Task> job)
            => job().GetAwaiter().GetResult();

        public void Start(int workerCount) { }

        public Task StopAsync()
            => Task.CompletedTask;
    }

    private sealed class FakeMapHandler : IMapHandler
    {
        public List<string> Calls { get; } = new();

        public void OnCurrentMapChanged(LyQuestMap? oldMap, LyQuestMap newMap)
            => Calls.Add("change");

        public void OnMapRegistered(LyQuestMap map)
            => Calls.Add("register");

        public void OnMapUnregistered(LyQuestMap map)
            => Calls.Add("unregister");

        public void Reset()
            => Calls.Clear();
    }

    [Test]
    public async Task GenerateMapAsync_SetsCurrentMap_AndNotifiesHandlers()
    {
        var map = new LyQuestMap(10, 10);
        var dungeonMap = new LyQuestMap(20, 20);
        var mapGenerator = Substitute.For<IMapGenerator>();
        mapGenerator.GenerateMapAsync().Returns(Task.FromResult(map));
        mapGenerator.GenerateDungeonMapAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(dungeonMap));
        var jobScheduler = new ImmediateJobScheduler();
        var worldManager = new WorldManager(mapGenerator, jobScheduler);
        var handler = new FakeMapHandler();
        worldManager.RegisterMapHandler(handler);

        await worldManager.GenerateMapAsync();

        Assert.That(worldManager.CurrentMap, Is.EqualTo(map));
        Assert.That(handler.Calls, Does.Contain("register"));
        Assert.That(handler.Calls, Does.Contain("change"));
    }

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

        worldManager.CurrentMap = new(10, 10);

        Assert.That(handler.Calls, Is.Empty);
    }

    [Test]
    public void UnregisterMapHandler_WithCurrentMap_UnregistersCurrentMap()
    {
        var mapGenerator = Substitute.For<IMapGenerator>();
        var jobScheduler = Substitute.For<IJobScheduler>();
        var worldManager = new WorldManager(mapGenerator, jobScheduler);
        var handler = new FakeMapHandler();
        var map = new LyQuestMap(10, 10);

        worldManager.RegisterMapHandler(handler);
        worldManager.CurrentMap = map;
        handler.Reset();

        worldManager.UnregisterMapHandler(handler);

        Assert.That(handler.Calls, Is.EqualTo(new[] { "unregister" }));
    }
}
