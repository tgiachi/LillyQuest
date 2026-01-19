using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.GameObjects.Features;
using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Tests;

/// <summary>
/// Tests for GameEntity implementation
/// </summary>
public class GameEntityTests
{
    private sealed class TestFeature : IGameObjectFeature
    {
        public bool IsEnabled { get; set; } = true;
    }

    [Test]
    public void Constructor_SetsIdAndName()
    {
        var entity = new GameEntity(42, "TestEntity");

        Assert.That(entity.Id, Is.EqualTo(42u));
        Assert.That(entity.Name, Is.EqualTo("TestEntity"));
    }

    [Test]
    public void Order_DefaultZero()
    {
        var entity = new GameEntity(1, "Entity");

        Assert.That(entity.Order, Is.EqualTo(0));
    }

    [Test]
    public void Order_IsMutable()
    {
        var entity = new GameEntity(1, "Entity");

        entity.Order = 100;

        Assert.That(entity.Order, Is.EqualTo(100));
    }

    [Test]
    public void Features_IsEnumerable()
    {
        var entity = new GameEntity(1, "Entity");
        var feature = new TestFeature();

        entity.AddFeature(feature);

        var features = entity.Features.ToList();
        Assert.That(features, Contains.Item(feature));
    }

    [Test]
    public void AddFeature_AddsFeatureToCollection()
    {
        var entity = new GameEntity(1, "Entity");
        var feature = new TestFeature();

        entity.AddFeature(feature);

        var retrieved = entity.GetFeature<TestFeature>();
        Assert.That(retrieved, Is.SameAs(feature));
    }

    [Test]
    public void GetFeature_ReturnsFeatureOfType()
    {
        var entity = new GameEntity(1, "Entity");
        var feature = new TestFeature();
        entity.AddFeature(feature);

        var retrieved = entity.GetFeature<TestFeature>();

        Assert.That(retrieved, Is.SameAs(feature));
    }

    [Test]
    public void GetFeature_NotFound_ReturnsNull()
    {
        var entity = new GameEntity(1, "Entity");

        var retrieved = entity.GetFeature<TestFeature>();

        Assert.That(retrieved, Is.Null);
    }

    [Test]
    public void HasFeature_WhenPresent_ReturnsTrue()
    {
        var entity = new GameEntity(1, "Entity");
        entity.AddFeature(new TestFeature());

        Assert.That(entity.HasFeature<TestFeature>(), Is.True);
    }

    [Test]
    public void HasFeature_WhenNotPresent_ReturnsFalse()
    {
        var entity = new GameEntity(1, "Entity");

        Assert.That(entity.HasFeature<TestFeature>(), Is.False);
    }

    [Test]
    public void TryGetFeature_WhenPresent_ReturnsTrue()
    {
        var entity = new GameEntity(1, "Entity");
        var feature = new TestFeature();
        entity.AddFeature(feature);

        var result = entity.TryGetFeature<TestFeature>(out var retrieved);

        Assert.That(result, Is.True);
        Assert.That(retrieved, Is.SameAs(feature));
    }

    [Test]
    public void TryGetFeature_WhenNotPresent_ReturnsFalse()
    {
        var entity = new GameEntity(1, "Entity");

        var result = entity.TryGetFeature<TestFeature>(out var retrieved);

        Assert.That(result, Is.False);
        Assert.That(retrieved, Is.Null);
    }

    [Test]
    public void RemoveFeature_RemovesFromCollection()
    {
        var entity = new GameEntity(1, "Entity");
        var feature = new TestFeature();
        entity.AddFeature(feature);

        var result = entity.RemoveFeature(feature);

        Assert.That(result, Is.True);
        Assert.That(entity.GetFeature<TestFeature>(), Is.Null);
    }

    [Test]
    public void Parent_CanBeSet()
    {
        var parent = new GameEntity(1, "Parent");
        var child = new GameEntity(2, "Child");

        child.Parent = parent;

        Assert.That(child.Parent, Is.SameAs(parent));
    }

    [Test]
    public void Parent_AddsChildToParentChildren()
    {
        var parent = new GameEntity(1, "Parent");
        var child = new GameEntity(2, "Child");

        child.Parent = parent;

        var children = parent.Children.ToList();
        Assert.That(children, Contains.Item(child));
    }

    [Test]
    public void SettingParentToNull_RemovesFromPreviousParent()
    {
        var parent = new GameEntity(1, "Parent");
        var child = new GameEntity(2, "Child");

        child.Parent = parent;
        child.Parent = null;

        var children = parent.Children.ToList();
        Assert.That(children, Does.Not.Contain(child));
    }

    [Test]
    public void ChangingParent_RemovesFromOldAndAddsToNew()
    {
        var parent1 = new GameEntity(1, "Parent1");
        var parent2 = new GameEntity(2, "Parent2");
        var child = new GameEntity(3, "Child");

        child.Parent = parent1;
        child.Parent = parent2;

        var children1 = parent1.Children.ToList();
        var children2 = parent2.Children.ToList();

        Assert.That(children1, Does.Not.Contain(child));
        Assert.That(children2, Contains.Item(child));
    }

}
