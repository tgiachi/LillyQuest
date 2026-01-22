using System.Reflection;
using DryIoc;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Systems;
using LillyQuest.Engine.Managers.Entities;
using LillyQuest.Engine.Managers.Services;
using LillyQuest.Engine.Types;

namespace LillyQuest.Tests.Engine;

public class SystemManagerTests
{
    private static readonly string[] UpdateOrder = { "A", "B" };
    private static readonly string[] RenderTwice = { "Render", "Render" };

    private sealed class RecordingSystem : ISystem
    {
        private readonly List<string> _calls;

        public RecordingSystem(string name, uint order, SystemQueryType queryType, List<string> calls)
        {
            Name = name;
            Order = order;
            QueryType = queryType;
            _calls = calls;
        }

        public uint Order { get; }
        public string Name { get; }
        public SystemQueryType QueryType { get; }

        public void Initialize() { }

        public void ProcessEntities(GameTime gameTime, IGameEntityManager entityManager)
        {
            _calls.Add(Name);
        }
    }

    [Test]
    public void RemoveSystem_RemovesFromAllQueryTypes()
    {
        var bootstrap = new LillyQuestBootstrap(new());
        var entityManager = new GameEntityManager(CreateContainer());
        var systemManager = new SystemManager(bootstrap, entityManager);

        var calls = new List<string>();
        var system = new RecordingSystem("Update", 0, SystemQueryType.Updateable | SystemQueryType.FixedUpdateable, calls);

        systemManager.RegisterSystem(system);
        systemManager.RemoveSystem(system);

        RaiseEvent(bootstrap, "Update", new());
        RaiseEvent(bootstrap, "FixedUpdate", new());

        Assert.That(calls, Is.Empty);
    }

    [Test]
    public void RenderEvent_ProcessesRenderableAndDebugRenderableSystems()
    {
        var bootstrap = new LillyQuestBootstrap(new());
        var entityManager = new GameEntityManager(CreateContainer());
        var systemManager = new SystemManager(bootstrap, entityManager);

        var calls = new List<string>();
        var system = new RecordingSystem("Render", 0, SystemQueryType.Renderable | SystemQueryType.DebugRenderable, calls);

        systemManager.RegisterSystem(system);

        RaiseEvent(bootstrap, "Render", new());

        Assert.That(calls, Is.EqualTo(RenderTwice));
    }

    [Test]
    public void UpdateEvent_ProcessesUpdateableSystemsInOrder()
    {
        var bootstrap = new LillyQuestBootstrap(new());
        var entityManager = new GameEntityManager(CreateContainer());
        var systemManager = new SystemManager(bootstrap, entityManager);

        var calls = new List<string>();
        var systemB = new RecordingSystem("B", 2, SystemQueryType.Updateable, calls);
        var systemA = new RecordingSystem("A", 1, SystemQueryType.Updateable, calls);

        systemManager.RegisterSystem(systemB);
        systemManager.RegisterSystem(systemA);

        RaiseEvent(bootstrap, "Update", new());

        Assert.That(calls, Is.EqualTo(UpdateOrder));
    }

    private static Container CreateContainer()
        => new();

    private static void RaiseEvent(LillyQuestBootstrap bootstrap, string eventName, GameTime gameTime)
    {
        var field = typeof(LillyQuestBootstrap).GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic);
        var handler = field?.GetValue(bootstrap) as LillyQuestBootstrap.TickEventHandler;

        if (handler is null)
        {
            Assert.Fail($"Event '{eventName}' was not found.");
        }

        handler.Invoke(gameTime);
    }
}
