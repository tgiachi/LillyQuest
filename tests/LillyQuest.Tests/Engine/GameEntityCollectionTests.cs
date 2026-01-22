using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Engine.Collections;
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Features;

namespace LillyQuest.Tests.Engine;

public class GameEntityCollectionTests
{
    private sealed class TestEntity : IGameEntity
    {
        public TestEntity(string name, uint order)
        {
            Name = name;
            Order = order;
        }

        public uint Id { get; set; }
        public uint Order { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public IList<IGameEntity> Children { get; set; } = new List<IGameEntity>();
        public IGameEntity? Parent { get; set; }

        public void Initialize() { }
    }

    private sealed class TestRenderableEntity : IGameEntity, IRenderableEntity
    {
        public TestRenderableEntity(string name, uint order)
        {
            Name = name;
            Order = order;
        }

        public uint Id { get; set; }
        public uint Order { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public IList<IGameEntity> Children { get; set; } = new List<IGameEntity>();
        public IGameEntity? Parent { get; set; }

        public void Initialize() { }

        public void Render(SpriteBatch spriteBatch, EngineRenderContext context) { }
    }

    [Test]
    public void Add_SameEntityTwice_DoesNotDuplicate()
    {
        var collection = new GameEntityCollection();
        var root = new TestEntity("root", 0);

        collection.Add(root);
        collection.Add(root);

        Assert.That(
            collection.OrderedEntities,
            Is.EqualTo(
                new IGameEntity[]
                {
                    root
                }
            )
        );
    }

    [Test]
    public void Add_WithMultipleRoots_SortsByOrderThenInsertion()
    {
        var collection = new GameEntityCollection();

        var root1 = new TestEntity("root1", 1);
        var root2 = new TestEntity("root2", 1);
        var root0 = new TestEntity("root0", 0);

        collection.Add(root1);
        collection.Add(root2);
        collection.Add(root0);

        Assert.That(
            collection.OrderedEntities,
            Is.EqualTo(
                new IGameEntity[]
                {
                    root0,
                    root1,
                    root2
                }
            )
        );
    }

    [Test]
    public void Add_WithNestedChildren_BuildsPreOrderList()
    {
        var collection = new GameEntityCollection();

        var root = new TestEntity("root", 0);
        var childA = new TestEntity("childA", 1);
        var childB = new TestEntity("childB", 2);
        var grandChild = new TestEntity("grandChild", 0);

        childA.Children.Add(grandChild);
        root.Children.Add(childB);
        root.Children.Add(childA);

        collection.Add(root);

        Assert.That(
            collection.OrderedEntities,
            Is.EqualTo(
                new IGameEntity[]
                {
                    root,
                    childA,
                    grandChild,
                    childB
                }
            )
        );
    }

    [Test]
    public void Add_WithNewParent_ReparentsEntity()
    {
        var collection = new GameEntityCollection();
        var rootA = new TestEntity("rootA", 0);
        var rootB = new TestEntity("rootB", 0);
        var child = new TestEntity("child", 0);

        collection.Add(rootA);
        collection.Add(rootB);
        collection.Add(child, rootA);
        collection.Add(child, rootB);

        Assert.That(rootA.Children, Does.Not.Contain(child));
        Assert.That(rootB.Children, Does.Contain(child));
        Assert.That(
            collection.OrderedEntities,
            Is.EqualTo(
                new IGameEntity[]
                {
                    rootA,
                    rootB,
                    child
                }
            )
        );
    }

    [Test]
    public void Add_WithParent_AttachesAndRebuilds()
    {
        var collection = new GameEntityCollection();

        var root = new TestEntity("root", 0);
        collection.Add(root);

        var child = new TestEntity("child", 0);
        collection.Add(child, root);

        Assert.That(child.Parent, Is.EqualTo(root));
        Assert.That(root.Children, Does.Contain(child));
        Assert.That(
            collection.OrderedEntities,
            Is.EqualTo(
                new IGameEntity[]
                {
                    root,
                    child
                }
            )
        );
    }

    [Test]
    public void Add_WithParent_WhenAlreadyChild_DoesNotDuplicate()
    {
        var collection = new GameEntityCollection();
        var root = new TestEntity("root", 0);
        var child = new TestEntity("child", 0);
        root.Children.Add(child);

        collection.Add(root);
        collection.Add(child, root);

        Assert.That(root.Children.Count(entity => ReferenceEquals(entity, child)), Is.EqualTo(1));
        Assert.That(
            collection.OrderedEntities,
            Is.EqualTo(
                new IGameEntity[]
                {
                    root,
                    child
                }
            )
        );
    }

    [Test]
    public void Add_WithSameOrderChildren_IsStableByInsertion()
    {
        var collection = new GameEntityCollection();

        var root = new TestEntity("root", 0);
        var childB = new TestEntity("childB", 1);
        var childA = new TestEntity("childA", 1);

        root.Children.Add(childB);
        root.Children.Add(childA);

        collection.Add(root);

        Assert.That(
            collection.OrderedEntities,
            Is.EqualTo(
                new IGameEntity[]
                {
                    root,
                    childB,
                    childA
                }
            )
        );
    }

    [Test]
    public void GetQueryOf_InvalidatesCacheOnChange()
    {
        var collection = new GameEntityCollection();
        var root = new TestEntity("root", 0);
        var renderableChild = new TestRenderableEntity("renderable", 0);
        root.Children.Add(renderableChild);
        collection.Add(root);

        var firstQuery = collection.GetQueryOf<IRenderableEntity>();

        var newRenderable = new TestRenderableEntity("renderable2", 0);
        collection.Add(newRenderable, root);

        var updatedQuery = collection.GetQueryOf<IRenderableEntity>();

        Assert.That(updatedQuery, Is.EqualTo(new[] { renderableChild, newRenderable }));
        Assert.That(ReferenceEquals(firstQuery, updatedQuery), Is.False);
    }

    [Test]
    public void GetQueryOf_ReturnsMatchingEntitiesAndCachesPerType()
    {
        var collection = new GameEntityCollection();
        var root = new TestEntity("root", 0);
        var renderableChild = new TestRenderableEntity("renderable", 0);
        var plainChild = new TestEntity("plain", 0);

        root.Children.Add(renderableChild);
        root.Children.Add(plainChild);

        collection.Add(root);

        var firstQuery = collection.GetQueryOf<IRenderableEntity>();
        var secondQuery = collection.GetQueryOf<IRenderableEntity>();

        Assert.That(firstQuery, Is.EqualTo(new[] { renderableChild }));
        Assert.That(ReferenceEquals(firstQuery, secondQuery), Is.True);
    }
}
