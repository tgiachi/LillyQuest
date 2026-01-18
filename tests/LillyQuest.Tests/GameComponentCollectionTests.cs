using LillyQuest.Engine.Collections;
using LillyQuest.Engine.Interfaces.Components;
using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Tests;

/// <summary>
/// Tests for GameComponentCollection high-performance component storage
/// </summary>
public class GameComponentCollectionTests
{
    private sealed class TestComponent : IGameComponent
    {
        public IGameEntity? Owner { get; set; }
        public void Initialize() { }
    }

    private sealed class AnotherComponent : IGameComponent
    {
        public IGameEntity? Owner { get; set; }
        public void Initialize() { }
    }

    [Test]
    public void Add_SingleComponent_CanRetrieveViaGetComponent()
    {
        var collection = new GameComponentCollection();
        var component = new TestComponent();

        collection.Add(component);

        var retrieved = collection.GetComponent<TestComponent>();
        Assert.That(retrieved, Is.SameAs(component));
    }

    [Test]
    public void Add_DuplicateComponentType_ThrowsInvalidOperationException()
    {
        var collection = new GameComponentCollection();
        var component1 = new TestComponent();
        var component2 = new TestComponent();

        collection.Add(component1);

        Assert.Throws<InvalidOperationException>(() => collection.Add(component2));
    }

    [Test]
    public void GetComponent_NotFound_ReturnsNull()
    {
        var collection = new GameComponentCollection();

        var result = collection.GetComponent<TestComponent>();

        Assert.That(result, Is.Null);
    }

    [Test]
    public void HasComponent_WhenPresent_ReturnsTrue()
    {
        var collection = new GameComponentCollection();
        var component = new TestComponent();

        collection.Add(component);

        Assert.That(collection.HasComponent<TestComponent>(), Is.True);
    }

    [Test]
    public void HasComponent_WhenNotPresent_ReturnsFalse()
    {
        var collection = new GameComponentCollection();

        Assert.That(collection.HasComponent<TestComponent>(), Is.False);
    }

    [Test]
    public void Remove_ExistingComponent_ReturnsTrue()
    {
        var collection = new GameComponentCollection();
        var component = new TestComponent();
        collection.Add(component);

        var result = collection.Remove(component);

        Assert.That(result, Is.True);
        Assert.That(collection.GetComponent<TestComponent>(), Is.Null);
    }

    [Test]
    public void Remove_NonExistentComponent_ReturnsFalse()
    {
        var collection = new GameComponentCollection();
        var component = new TestComponent();

        var result = collection.Remove(component);

        Assert.That(result, Is.False);
    }

    [Test]
    public void Clear_WithComponents_MakesCollectionEmpty()
    {
        var collection = new GameComponentCollection();
        collection.Add(new TestComponent());
        collection.Add(new AnotherComponent());

        collection.Clear();

        Assert.That(collection.Count, Is.Zero);
        Assert.That(collection.GetComponent<TestComponent>(), Is.Null);
        Assert.That(collection.GetComponent<AnotherComponent>(), Is.Null);
    }

    [Test]
    public void Enumeration_ContainsAllAddedComponents()
    {
        var collection = new GameComponentCollection();
        var component1 = new TestComponent();
        var component2 = new AnotherComponent();
        collection.Add(component1);
        collection.Add(component2);

        var components = collection.ToList();

        Assert.That(components, Has.Count.EqualTo(2));
        Assert.That(components, Contains.Item(component1));
        Assert.That(components, Contains.Item(component2));
    }

    [Test]
    public void Count_ReflectsComponentCount()
    {
        var collection = new GameComponentCollection();
        var testComponent = new TestComponent();
        var anotherComponent = new AnotherComponent();

        Assert.That(collection.Count, Is.Zero);

        collection.Add(testComponent);
        Assert.That(collection.Count, Is.EqualTo(1));

        collection.Add(anotherComponent);
        Assert.That(collection.Count, Is.EqualTo(2));

        collection.Remove(testComponent);
        Assert.That(collection.Count, Is.EqualTo(1));
    }
}
