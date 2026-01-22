using System.Numerics;
using LillyQuest.Engine.Screens.UI;
using Silk.NET.Input;

namespace LillyQuest.Tests.Engine.UI;

public class UIRootScreenZIndexTests
{
    [Test]
    public void OnMouseDown_BringsWindowToFront()
    {
        var screen = new UIRootScreen();
        var back = new UIWindow
        {
            Position = new(0, 0),
            Size = new(100, 100),
            ZIndex = 1
        };
        var front = new UIWindow
        {
            Position = new(200, 0),
            Size = new(100, 100),
            ZIndex = 5
        };

        screen.Root.Add(back);
        screen.Root.Add(front);

        var handled = screen.OnMouseDown(10, 10, Array.Empty<MouseButton>());

        Assert.That(handled, Is.True);
        Assert.That(back.ZIndex, Is.GreaterThan(front.ZIndex));
    }
}
