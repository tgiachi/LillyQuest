using LillyQuest.RogueLike.GameObjects;

namespace LillyQuest.Tests.RogueLike.GameObjects;

public class CreatureGameObjectTests
{
    [Test]
    public void DefaultCreature_IsTransparent()
    {
        var creature = new CreatureGameObject(new(0, 0));

        Assert.That(creature.IsTransparent, Is.True);
    }
}
