using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Components;

namespace LillyQuest.Tests.RogueLike.Components;

public class LightBackgroundComponentTests
{
    [Test]
    public void LightBackgroundComponent_StoresValues()
    {
        var start = LyColor.Orange;
        var end = LyColor.Transparent;

        var component = new LightBackgroundComponent(start, end);

        Assert.That(component.StartBackground, Is.EqualTo(start));
        Assert.That(component.EndBackground, Is.EqualTo(end));
    }
}
