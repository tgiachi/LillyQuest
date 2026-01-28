using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Components;
using NUnit.Framework;

namespace LillyQuest.Tests.RogueLike.Components;

public class LightSourceComponentTests
{
    [Test]
    public void LightSourceComponent_StoresValues()
    {
        var start = LyColor.Yellow;
        var end = LyColor.Black;

        var component = new LightSourceComponent(radius: 4, startColor: start, endColor: end);

        Assert.That(component.Radius, Is.EqualTo(4));
        Assert.That(component.StartColor, Is.EqualTo(start));
        Assert.That(component.EndColor, Is.EqualTo(end));
    }
}
