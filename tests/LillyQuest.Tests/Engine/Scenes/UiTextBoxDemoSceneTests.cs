using LillyQuest.Game.Scenes;

namespace LillyQuest.Tests.Engine.Scenes;

public class UiTextBoxDemoSceneTests
{
    [Test]
    public void UiTextBoxDemoScene_CanBeConstructed()
    {
        var scene = new UiTextBoxDemoScene(null!, null!, null!, null!, null!);
        Assert.That(scene, Is.Not.Null);
    }
}
