using System.Numerics;
using LillyQuest.Engine.Screens.UI;
using NUnit.Framework;

namespace LillyQuest.Tests.Engine.UI;

public class UIWindowTests
{
    [Test]
    public void MouseDown_TitleBar_StartsDrag_WhenMovable()
    {
        var window = new UIWindow
        {
            Position = Vector2.Zero,
            Size = new Vector2(100, 50),
            IsTitleBarEnabled = true,
            IsWindowMovable = true
        };
        window.TitleBarHeight = 10f;

        var handled = window.HandleMouseDown(new Vector2(5, 5));
        var moved = window.HandleMouseMove(new Vector2(20, 20));

        Assert.That(handled, Is.True);
        Assert.That(moved, Is.True);
        Assert.That(window.Position, Is.EqualTo(new Vector2(15, 15)));
    }
}
