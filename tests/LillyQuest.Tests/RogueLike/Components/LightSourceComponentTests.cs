using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Components;

namespace LillyQuest.Tests.RogueLike.Components;

public class LightSourceComponentTests
{
    [Test]
    public void LightSourceComponent_StoresValues()
    {
        var start = LyColor.Yellow;
        var end = LyColor.Black;

        var component = new LightSourceComponent(4, start, end);

        Assert.That(component.Radius, Is.EqualTo(4));
        Assert.That(component.StartColor, Is.EqualTo(start));
        Assert.That(component.EndColor, Is.EqualTo(end));
    }
}
