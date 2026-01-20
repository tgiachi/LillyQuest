using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Managers.Entities;

namespace LillyQuest.Tests;

public class GameEntityManagerTests
{
    private sealed class RenderableTestEntity : GameEntity, IRenderableEntity
    {
        public void Render(SpriteBatch spriteBatch, EngineRenderContext context) { }
    }

    [Test]
    public void AddEntity_AssignsId_WhenZero()
    {
        var manager = new GameEntityManager();
        var first = new GameEntity();
        var second = new GameEntity();

        manager.AddEntity(first);
        manager.AddEntity(second);

        Assert.That(first.Id, Is.EqualTo(1));
        Assert.That(second.Id, Is.EqualTo(2));
    }

    [Test]
    public void AddEntity_DuplicateIdDifferentEntity_Throws()
    {
        var manager = new GameEntityManager();
        var first = new GameEntity { Id = 10 };
        var second = new GameEntity { Id = 10 };

        manager.AddEntity(first);

        Assert.Throws<InvalidOperationException>(() => manager.AddEntity(second));
    }

    [Test]
    public void AddEntity_WithHierarchy_AssignsIdsToChildren()
    {
        var manager = new GameEntityManager();
        var root = new GameEntity();
        var child = new GameEntity();
        root.Children.Add(child);

        manager.AddEntity(root);

        Assert.That(child.Id, Is.Not.EqualTo(0));
        Assert.That(manager.GetEntityById(child.Id), Is.SameAs(child));
    }

    [Test]
    public void AddEntity_WithParent_AttachesAndOrders()
    {
        var manager = new GameEntityManager();
        var root = new GameEntity { Order = 0 };
        var child = new GameEntity { Order = 0 };

        manager.AddEntity(root);
        manager.AddEntity(child, root);

        Assert.That(child.Parent, Is.SameAs(root));
        Assert.That(root.Children, Does.Contain(child));
        Assert.That(manager.OrderedEntities, Is.EqualTo(new IGameEntity[] { root, child }));
    }

    [Test]
    public void GetQueryOf_UsesCollectionCache()
    {
        var manager = new GameEntityManager();
        var root = new GameEntity();
        var renderable = new RenderableTestEntity();
        root.Children.Add(renderable);
        manager.AddEntity(root);

        var first = manager.GetQueryOf<IRenderableEntity>();
        var second = manager.GetQueryOf<IRenderableEntity>();

        Assert.That(first, Is.EqualTo(new IRenderableEntity[] { renderable }));
        Assert.That(ReferenceEquals(first, second), Is.True);
    }

    [Test]
    public void RemoveEntity_RemovesSubtree()
    {
        var manager = new GameEntityManager();
        var root = new GameEntity();
        var child = new GameEntity();
        root.Children.Add(child);

        manager.AddEntity(root);
        manager.RemoveEntity(root);

        Assert.That(manager.GetEntityById(root.Id), Is.Null);
        Assert.That(manager.GetEntityById(child.Id), Is.Null);
        Assert.That(manager.OrderedEntities, Is.Empty);
    }
}
