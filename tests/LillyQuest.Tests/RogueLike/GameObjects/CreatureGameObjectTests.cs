using LillyQuest.RogueLike.GameObjects;
using NUnit.Framework;
using SadRogue.Primitives;

namespace LillyQuest.Tests.RogueLike.GameObjects;

public class CreatureGameObjectTests
{
    [Test]
    public void DefaultCreature_IsTransparent()
    {
        var creature = new CreatureGameObject(new Point(0, 0));

        Assert.That(creature.IsTransparent, Is.True);
    }
}
