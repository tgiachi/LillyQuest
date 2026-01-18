using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.Components;
using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Tests;

/// <summary>
/// Tests for GameEntity implementation
/// </summary>
public class GameEntityTests
{
    private sealed class TestComponent : IGameComponent
    {
        public IGameEntity? Owner { get; set; }
        public void Initialize() { }
    }

    [Test]
    public void Constructor_SetsIdAndName()
    {
        var entity = new GameEntity(42, "TestEntity");

        Assert.That(entity.Id, Is.EqualTo(42u));
        Assert.That(entity.Name, Is.EqualTo("TestEntity"));
    }

    [Test]
    public void IsActive_DefaultTrue()
    {
        var entity = new GameEntity(1, "Entity");

        Assert.That(entity.IsActive, Is.True);
    }

    [Test]
    public void IsActive_IsMutable()
    {
        var entity = new GameEntity(1, "Entity");

        entity.IsActive = false;

        Assert.That(entity.IsActive, Is.False);
    }

    [Test]
    public void Order_DefaultZero()
    {
        var entity = new GameEntity(1, "Entity");

        Assert.That(entity.Order, Is.EqualTo(0u));
    }

    [Test]
    public void Order_IsMutable()
    {
        var entity = new GameEntity(1, "Entity");

        entity.Order = 100u;

        Assert.That(entity.Order, Is.EqualTo(100u));
    }

    [Test]
    public void Components_IsEnumerable()
    {
        var entity = new GameEntity(1, "Entity");
        var component = new TestComponent();

        entity.AddComponent(component);

        var components = entity.Components.ToList();
        Assert.That(components, Contains.Item(component));
    }

    [Test]
    public void AddComponent_AddsComponentToCollection()
    {
        var entity = new GameEntity(1, "Entity");
        var component = new TestComponent();

        entity.AddComponent(component);

        var retrieved = entity.GetComponent<TestComponent>();
        Assert.That(retrieved, Is.SameAs(component));
    }

    [Test]
    public void GetComponent_ReturnsComponentOfType()
    {
        var entity = new GameEntity(1, "Entity");
        var component = new TestComponent();
        entity.AddComponent(component);

        var retrieved = entity.GetComponent<TestComponent>();

        Assert.That(retrieved, Is.SameAs(component));
    }

    [Test]
    public void GetComponent_NotFound_ReturnsNull()
    {
        var entity = new GameEntity(1, "Entity");

        var retrieved = entity.GetComponent<TestComponent>();

        Assert.That(retrieved, Is.Null);
    }

    [Test]
    public void HasComponent_WhenPresent_ReturnsTrue()
    {
        var entity = new GameEntity(1, "Entity");
        entity.AddComponent(new TestComponent());

        Assert.That(entity.HasComponent<TestComponent>(), Is.True);
    }

    [Test]
    public void HasComponent_WhenNotPresent_ReturnsFalse()
    {
        var entity = new GameEntity(1, "Entity");

        Assert.That(entity.HasComponent<TestComponent>(), Is.False);
    }

    [Test]
    public void AddComponent_SetsComponentOwner()
    {
        var entity = new GameEntity(1, "Entity");
        var component = new TestComponent();

        entity.AddComponent(component);

        Assert.That(component.Owner, Is.SameAs(entity));
    }

    [Test]
    public void AddComponent_CallsComponentInitialize()
    {
        var entity = new GameEntity(1, "Entity");
        var initializeCalled = false;

        var component = new TestComponentWithInitTrack(() => initializeCalled = true);
        entity.AddComponent(component);

        Assert.That(initializeCalled, Is.True);
    }

    private sealed class TestComponentWithInitTrack : IGameComponent
    {
        private readonly Action _onInitialize;

        public IGameEntity? Owner { get; set; }

        public TestComponentWithInitTrack(Action onInitialize)
        {
            _onInitialize = onInitialize;
        }

        public void Initialize() => _onInitialize();
    }

    [Test]
    public void RemoveComponent_RemovesFromCollection()
    {
        var entity = new GameEntity(1, "Entity");
        var component = new TestComponent();
        entity.AddComponent(component);

        var result = entity.RemoveComponent(component);

        Assert.That(result, Is.True);
        Assert.That(entity.GetComponent<TestComponent>(), Is.Null);
    }
}
