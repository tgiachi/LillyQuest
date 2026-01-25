using LillyQuest.Game.Scenes;

namespace LillyQuest.Tests.Engine.Scenes;

public class UiMenuDemoSceneTests
{
    [Test]
    public void UiMenuDemoScene_CanBeConstructed()
    {
        var scene = new UiMenuDemoScene(null!, null!);
        Assert.That(scene, Is.Not.Null);
    }
}
