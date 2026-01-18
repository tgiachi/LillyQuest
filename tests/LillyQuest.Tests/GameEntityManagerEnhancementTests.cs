using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Managers;
using LillyQuest.Engine.Interfaces.Components;
using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Tests;

/// <summary>
/// Tests for GameEntityManager ordered indexing and lifecycle events
/// </summary>
public class GameEntityManagerEnhancementTests
{
    private sealed class OrderedComponent : IGameComponent
    {
        public IGameEntity? Owner { get; set; }
        public void Initialize() { }
    }

    [Test]
    public void GetAllComponentsOfType_MultipleEntitiesDifferentOrders_ReturnsOrderedByEntityOrder()
    {
        var manager = new GameEntityManager();

        // Entity with Order 10
        var entity1 = new GameEntity(1, "Entity1");
        entity1.Order = 10u;
        var comp1 = new OrderedComponent();
        entity1.AddComponent(comp1);
        manager.AddEntity(entity1);

        // Entity with Order 5
        var entity2 = new GameEntity(2, "Entity2");
        entity2.Order = 5u;
        var comp2 = new OrderedComponent();
        entity2.AddComponent(comp2);
        manager.AddEntity(entity2);

        // Entity with Order 20
        var entity3 = new GameEntity(3, "Entity3");
        entity3.Order = 20u;
        var comp3 = new OrderedComponent();
        entity3.AddComponent(comp3);
        manager.AddEntity(entity3);

        var components = manager.GetAllComponentsOfType<OrderedComponent>().ToList();

        // Should be ordered by Entity.Order: 5, 10, 20
        Assert.That(components, Has.Count.EqualTo(3));
        Assert.That(components[0], Is.SameAs(comp2)); // Order 5
        Assert.That(components[1], Is.SameAs(comp1)); // Order 10
        Assert.That(components[2], Is.SameAs(comp3)); // Order 20
    }

    [Test]
    public void OnGameEntityAdded_FiredAfterAddEntity()
    {
        var manager = new GameEntityManager();
        var entity = new GameEntity(1, "TestEntity");
        var addedCalled = false;
        IGameEntity? addedEntity = null;

        manager.OnGameEntityAdded += (e) =>
        {
            addedCalled = true;
            addedEntity = e;
        };

        manager.AddEntity(entity);

        Assert.That(addedCalled, Is.True);
        Assert.That(addedEntity, Is.SameAs(entity));
    }

    [Test]
    public void OnGameEntityRemoved_FiredAfterRemoveEntity()
    {
        var manager = new GameEntityManager();
        var entity = new GameEntity(1, "TestEntity");
        manager.AddEntity(entity);

        var removedCalled = false;
        IGameEntity? removedEntity = null;

        manager.OnGameEntityRemoved += (e) =>
        {
            removedCalled = true;
            removedEntity = e;
        };

        manager.RemoveEntity(entity);

        Assert.That(removedCalled, Is.True);
        Assert.That(removedEntity, Is.SameAs(entity));
    }

    [Test]
    public void GetAllComponentsOfType_AfterRemoveEntity_RemainsOrdered()
    {
        var manager = new GameEntityManager();

        var entity1 = new GameEntity(1, "Entity1");
        entity1.Order = 10u;
        var comp1 = new OrderedComponent();
        entity1.AddComponent(comp1);
        manager.AddEntity(entity1);

        var entity2 = new GameEntity(2, "Entity2");
        entity2.Order = 5u;
        var comp2 = new OrderedComponent();
        entity2.AddComponent(comp2);
        manager.AddEntity(entity2);

        var entity3 = new GameEntity(3, "Entity3");
        entity3.Order = 20u;
        var comp3 = new OrderedComponent();
        entity3.AddComponent(comp3);
        manager.AddEntity(entity3);

        // Remove middle entity
        manager.RemoveEntity(entity1);

        var components = manager.GetAllComponentsOfType<OrderedComponent>().ToList();

        // Should still be ordered: 5, 20
        Assert.That(components, Has.Count.EqualTo(2));
        Assert.That(components[0], Is.SameAs(comp2)); // Order 5
        Assert.That(components[1], Is.SameAs(comp3)); // Order 20
    }

    [Test]
    public void AddEntity_MultipleEntitiesWithSameComponentType_AllOrderedByEntityOrder()
    {
        var manager = new GameEntityManager();

        // Entity1 with Order 20
        var entity1 = new GameEntity(1, "Entity1");
        entity1.Order = 20u;
        var comp1 = new OrderedComponent();
        entity1.AddComponent(comp1);
        manager.AddEntity(entity1);

        // Entity2 with Order 10
        var entity2 = new GameEntity(2, "Entity2");
        entity2.Order = 10u;
        var comp2 = new OrderedComponent();
        entity2.AddComponent(comp2);
        manager.AddEntity(entity2);

        // Entity3 with Order 15
        var entity3 = new GameEntity(3, "Entity3");
        entity3.Order = 15u;
        var comp3 = new OrderedComponent();
        entity3.AddComponent(comp3);
        manager.AddEntity(entity3);

        var components = manager.GetAllComponentsOfType<OrderedComponent>().ToList();

        // Should be ordered by entity Order: 10, 15, 20
        Assert.That(components, Has.Count.EqualTo(3));
        Assert.That(components[0], Is.SameAs(comp2)); // Order 10
        Assert.That(components[1], Is.SameAs(comp3)); // Order 15
        Assert.That(components[2], Is.SameAs(comp1)); // Order 20
    }

    [Test]
    public void OnGameEntityAdded_EventFiresAfterComponentsAreIndexed()
    {
        var manager = new GameEntityManager();
        var entity = new GameEntity(1, "Entity");
        entity.Order = 10u;
        var component = new OrderedComponent();
        entity.AddComponent(component);

        var componentsCountDuringEvent = -1;

        manager.OnGameEntityAdded += (e) =>
        {
            // During event, the component should already be indexed and available
            var components = manager.GetAllComponentsOfType<OrderedComponent>().ToList();
            componentsCountDuringEvent = components.Count;
        };

        manager.AddEntity(entity);

        Assert.That(componentsCountDuringEvent, Is.EqualTo(1));
    }

    [Test]
    public void MultipleListeners_BothReceiveEntityAddedEvent()
    {
        var manager = new GameEntityManager();
        var entity = new GameEntity(1, "Entity");

        var listener1Called = false;
        var listener2Called = false;

        manager.OnGameEntityAdded += (e) => listener1Called = true;
        manager.OnGameEntityAdded += (e) => listener2Called = true;

        manager.AddEntity(entity);

        Assert.That(listener1Called, Is.True);
        Assert.That(listener2Called, Is.True);
    }
}
