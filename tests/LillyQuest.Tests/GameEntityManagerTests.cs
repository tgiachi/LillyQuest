using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Managers;
using LillyQuest.Engine.Interfaces.Components;
using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Tests;

/// <summary>
/// Tests for GameEntityManager implementation
/// </summary>
public class GameEntityManagerTests
{
    private sealed class TransformComponent : IGameComponent
    {
        public IGameEntity? Owner { get; set; }
        public void Initialize() { }
    }

    private sealed class RenderComponent : IGameComponent
    {
        public IGameEntity? Owner { get; set; }
        public void Initialize() { }
    }

    private sealed class PhysicsComponent : IGameComponent
    {
        public IGameEntity? Owner { get; set; }
        public void Initialize() { }
    }

    [Test]
    public void AddEntity_AddsEntityToManager()
    {
        var manager = new GameEntityManager();
        var entity = new GameEntity(1, "TestEntity");

        manager.AddEntity(entity);

        var allComponents = manager.GetAllComponentsOfType<TransformComponent>();
        // Should not throw, entity is tracked
        Assert.That(allComponents, Is.Not.Null);
    }

    [Test]
    public void GetAllComponentsOfType_EmptyManager_ReturnsEmpty()
    {
        var manager = new GameEntityManager();

        var components = manager.GetAllComponentsOfType<TransformComponent>().ToList();

        Assert.That(components, Is.Empty);
    }

    [Test]
    public void GetAllComponentsOfType_SingleEntitySingleComponent_ReturnsThatComponent()
    {
        var manager = new GameEntityManager();
        var entity = new GameEntity(1, "Entity1");
        var transform = new TransformComponent();
        entity.AddComponent(transform);
        manager.AddEntity(entity);

        var components = manager.GetAllComponentsOfType<TransformComponent>().ToList();

        Assert.That(components, Has.Count.EqualTo(1));
        Assert.That(components[0], Is.SameAs(transform));
    }

    [Test]
    public void GetAllComponentsOfType_MultipleEntitiesSameComponent_ReturnsAll()
    {
        var manager = new GameEntityManager();
        var entity1 = new GameEntity(1, "Entity1");
        var entity2 = new GameEntity(2, "Entity2");

        var transform1 = new TransformComponent();
        var transform2 = new TransformComponent();

        entity1.AddComponent(transform1);
        entity2.AddComponent(transform2);

        manager.AddEntity(entity1);
        manager.AddEntity(entity2);

        var components = manager.GetAllComponentsOfType<TransformComponent>().ToList();

        Assert.That(components, Has.Count.EqualTo(2));
        Assert.That(components, Contains.Item(transform1));
        Assert.That(components, Contains.Item(transform2));
    }

    [Test]
    public void GetAllComponentsOfType_MultipleComponentTypes_ReturnOnlyRequested()
    {
        var manager = new GameEntityManager();
        var entity = new GameEntity(1, "Entity1");

        var transform = new TransformComponent();
        var render = new RenderComponent();

        entity.AddComponent(transform);
        entity.AddComponent(render);

        manager.AddEntity(entity);

        var transforms = manager.GetAllComponentsOfType<TransformComponent>().ToList();
        var renders = manager.GetAllComponentsOfType<RenderComponent>().ToList();

        Assert.That(transforms, Has.Count.EqualTo(1));
        Assert.That(transforms[0], Is.SameAs(transform));
        Assert.That(renders, Has.Count.EqualTo(1));
        Assert.That(renders[0], Is.SameAs(render));
    }

    [Test]
    public void RemoveEntity_RemovesEntityAndItsComponents()
    {
        var manager = new GameEntityManager();
        var entity = new GameEntity(1, "Entity1");
        var transform = new TransformComponent();
        entity.AddComponent(transform);
        manager.AddEntity(entity);

        manager.RemoveEntity(entity);

        var components = manager.GetAllComponentsOfType<TransformComponent>().ToList();
        Assert.That(components, Is.Empty);
    }

    [Test]
    public void RemoveEntity_KeepsOtherEntitiesComponents()
    {
        var manager = new GameEntityManager();
        var entity1 = new GameEntity(1, "Entity1");
        var entity2 = new GameEntity(2, "Entity2");

        var transform1 = new TransformComponent();
        var transform2 = new TransformComponent();

        entity1.AddComponent(transform1);
        entity2.AddComponent(transform2);

        manager.AddEntity(entity1);
        manager.AddEntity(entity2);

        manager.RemoveEntity(entity1);

        var components = manager.GetAllComponentsOfType<TransformComponent>().ToList();
        Assert.That(components, Has.Count.EqualTo(1));
        Assert.That(components[0], Is.SameAs(transform2));
    }

    [Test]
    public void GetAllComponentsOfType_ComplexScenario_CorrectlyIndexes()
    {
        var manager = new GameEntityManager();

        // Entity 1: Transform + Render
        var entity1 = new GameEntity(1, "Entity1");
        var t1 = new TransformComponent();
        var r1 = new RenderComponent();
        entity1.AddComponent(t1);
        entity1.AddComponent(r1);
        manager.AddEntity(entity1);

        // Entity 2: Transform + Physics
        var entity2 = new GameEntity(2, "Entity2");
        var t2 = new TransformComponent();
        var p2 = new PhysicsComponent();
        entity2.AddComponent(t2);
        entity2.AddComponent(p2);
        manager.AddEntity(entity2);

        // Entity 3: Render + Physics
        var entity3 = new GameEntity(3, "Entity3");
        var r3 = new RenderComponent();
        var p3 = new PhysicsComponent();
        entity3.AddComponent(r3);
        entity3.AddComponent(p3);
        manager.AddEntity(entity3);

        // Verify indices
        var transforms = manager.GetAllComponentsOfType<TransformComponent>().ToList();
        var renders = manager.GetAllComponentsOfType<RenderComponent>().ToList();
        var physics = manager.GetAllComponentsOfType<PhysicsComponent>().ToList();

        Assert.That(transforms, Has.Count.EqualTo(2));
        Assert.That(renders, Has.Count.EqualTo(2));
        Assert.That(physics, Has.Count.EqualTo(2));

        Assert.That(transforms, Contains.Item(t1));
        Assert.That(transforms, Contains.Item(t2));

        Assert.That(renders, Contains.Item(r1));
        Assert.That(renders, Contains.Item(r3));

        Assert.That(physics, Contains.Item(p2));
        Assert.That(physics, Contains.Item(p3));
    }
}
