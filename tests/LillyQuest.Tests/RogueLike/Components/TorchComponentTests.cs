using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Components;

namespace LillyQuest.Tests.RogueLike.Components;

public class TorchComponentTests
{
    [Test]
    public void TorchComponent_StoresValues()
    {
        var component = new TorchComponent(
            4,
            LyColor.Yellow,
            LyColor.Black,
            LyColor.Orange,
            LyColor.Transparent,
            204
        );

        Assert.That(component.Radius, Is.EqualTo(4));
        Assert.That(component.ForegroundStart, Is.EqualTo(LyColor.Yellow));
        Assert.That(component.ForegroundEnd, Is.EqualTo(LyColor.Black));
        Assert.That(component.BackgroundStart, Is.EqualTo(LyColor.Orange));
        Assert.That(component.BackgroundEnd, Is.EqualTo(LyColor.Transparent));
        Assert.That(component.BackgroundAlpha, Is.EqualTo(204));
    }
}
