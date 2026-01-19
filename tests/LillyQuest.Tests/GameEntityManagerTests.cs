using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Managers;
using LillyQuest.Engine.Interfaces.GameObjects.Features;
using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Tests;

/// <summary>
/// Tests for GameEntityManager implementation
/// </summary>
public class GameEntityManagerTests
{
    private sealed class TransformFeature : IGameObjectFeature
    {
        public bool IsEnabled { get; set; } = true;
    }

    private sealed class RenderFeature : IGameObjectFeature
    {
        public bool IsEnabled { get; set; } = true;
    }

    private sealed class PhysicsFeature : IGameObjectFeature
    {
        public bool IsEnabled { get; set; } = true;
    }

    [Test]
    public void AddEntity_AddsEntityToManager()
    {
        var manager = new GameEntityManager();
        var entity = new GameEntity(1, "TestEntity");

        manager.AddEntity(entity);

        var allFeatures = manager.QueryOfType<TransformFeature>();
        // Should not throw, entity is tracked
        Assert.That(allFeatures, Is.Not.Null);
    }

    [Test]
    public void QueryOfType_EmptyManager_ReturnsEmpty()
    {
        var manager = new GameEntityManager();

        var features = manager.QueryOfType<TransformFeature>().ToList();

        Assert.That(features, Is.Empty);
    }

    [Test]
    public void QueryOfType_SingleEntitySingleFeature_ReturnsThatFeature()
    {
        var manager = new GameEntityManager();
        var entity = new GameEntity(1, "Entity1");
        var transform = new TransformFeature();
        entity.AddFeature(transform);
        manager.AddEntity(entity);

        var features = manager.QueryOfType<TransformFeature>().ToList();

        Assert.That(features, Has.Count.EqualTo(1));
        Assert.That(features[0], Is.SameAs(transform));
    }

    [Test]
    public void QueryOfType_MultipleEntitiesSameFeature_ReturnsAll()
    {
        var manager = new GameEntityManager();
        var entity1 = new GameEntity(1, "Entity1");
        var entity2 = new GameEntity(2, "Entity2");

        var transform1 = new TransformFeature();
        var transform2 = new TransformFeature();

        entity1.AddFeature(transform1);
        entity2.AddFeature(transform2);

        manager.AddEntity(entity1);
        manager.AddEntity(entity2);

        var features = manager.QueryOfType<TransformFeature>().ToList();

        Assert.That(features, Has.Count.EqualTo(2));
        Assert.That(features, Contains.Item(transform1));
        Assert.That(features, Contains.Item(transform2));
    }

    [Test]
    public void QueryOfType_MultipleFeatureTypes_ReturnOnlyRequested()
    {
        var manager = new GameEntityManager();
        var entity = new GameEntity(1, "Entity1");

        var transform = new TransformFeature();
        var render = new RenderFeature();

        entity.AddFeature(transform);
        entity.AddFeature(render);

        manager.AddEntity(entity);

        var transforms = manager.QueryOfType<TransformFeature>().ToList();
        var renders = manager.QueryOfType<RenderFeature>().ToList();

        Assert.That(transforms, Has.Count.EqualTo(1));
        Assert.That(transforms[0], Is.SameAs(transform));
        Assert.That(renders, Has.Count.EqualTo(1));
        Assert.That(renders[0], Is.SameAs(render));
    }

    [Test]
    public void RemoveEntity_RemovesEntityAndItsFeatures()
    {
        var manager = new GameEntityManager();
        var entity = new GameEntity(1, "Entity1");
        var transform = new TransformFeature();
        entity.AddFeature(transform);
        manager.AddEntity(entity);

        manager.RemoveEntity(entity);

        var features = manager.QueryOfType<TransformFeature>().ToList();
        Assert.That(features, Is.Empty);
    }

    [Test]
    public void RemoveEntity_KeepsOtherEntitiesFeatures()
    {
        var manager = new GameEntityManager();
        var entity1 = new GameEntity(1, "Entity1");
        var entity2 = new GameEntity(2, "Entity2");

        var transform1 = new TransformFeature();
        var transform2 = new TransformFeature();

        entity1.AddFeature(transform1);
        entity2.AddFeature(transform2);

        manager.AddEntity(entity1);
        manager.AddEntity(entity2);

        manager.RemoveEntity(entity1);

        var features = manager.QueryOfType<TransformFeature>().ToList();
        Assert.That(features, Has.Count.EqualTo(1));
        Assert.That(features[0], Is.SameAs(transform2));
    }

    [Test]
    public void QueryOfType_ComplexScenario_CorrectlyIndexes()
    {
        var manager = new GameEntityManager();

        // Entity 1: Transform + Render
        var entity1 = new GameEntity(1, "Entity1");
        var t1 = new TransformFeature();
        var r1 = new RenderFeature();
        entity1.AddFeature(t1);
        entity1.AddFeature(r1);
        manager.AddEntity(entity1);

        // Entity 2: Transform + Physics
        var entity2 = new GameEntity(2, "Entity2");
        var t2 = new TransformFeature();
        var p2 = new PhysicsFeature();
        entity2.AddFeature(t2);
        entity2.AddFeature(p2);
        manager.AddEntity(entity2);

        // Entity 3: Render + Physics
        var entity3 = new GameEntity(3, "Entity3");
        var r3 = new RenderFeature();
        var p3 = new PhysicsFeature();
        entity3.AddFeature(r3);
        entity3.AddFeature(p3);
        manager.AddEntity(entity3);

        // Verify indices
        var transforms = manager.QueryOfType<TransformFeature>().ToList();
        var renders = manager.QueryOfType<RenderFeature>().ToList();
        var physics = manager.QueryOfType<PhysicsFeature>().ToList();

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
