using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.GameObjects;
using NUnit.Framework;
using SadRogue.Primitives;

namespace LillyQuest.Tests.RogueLike.GameObjects;

public class IViewportUpdateableTests
{
    [Test]
    public void ViewportUpdateable_Interface_CanBeImplemented()
    {
        var obj = new TestViewportObject(new Point(1, 1));
        obj.Update(new GameTime());

        Assert.That(obj.UpdateCount, Is.EqualTo(1));
    }

    private sealed class TestViewportObject : CreatureGameObject, IViewportUpdateable
    {
        public int UpdateCount { get; private set; }

        public TestViewportObject(Point position) : base(position) { }

        public void Update(GameTime gameTime)
            => UpdateCount++;
    }
}
