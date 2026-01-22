using System.Numerics;
using LillyQuest.Engine.Screens.UI;

namespace LillyQuest.Tests.Engine.UI;

public class UIBackgroundControlTests
{
    [Test]
    public void Input_Is_Always_Ignored()
    {
        var background = new UIBackgroundControl
        {
            Position = Vector2.Zero,
            Size = new(100, 100)
        };

        Assert.That(background.HandleMouseDown(new Vector2(10, 10)), Is.False);
        Assert.That(background.HandleMouseMove(new Vector2(10, 10)), Is.False);
        Assert.That(background.HandleMouseUp(new Vector2(10, 10)), Is.False);
    }
}
