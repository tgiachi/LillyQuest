using LillyQuest.Game.Scenes;

namespace LillyQuest.Tests.Engine.Scenes;

public class UiWidgetsDemoSceneTests
{
    [Test]
    public void UiWidgetsDemoScene_CanBeConstructed()
    {
        var scene = new UiWidgetsDemoScene(null!, null!, null!);
        Assert.That(scene, Is.Not.Null);
    }
}
