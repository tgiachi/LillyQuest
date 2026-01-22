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

    [Test]
    public void Drag_Clamps_To_Parent_Bounds()
    {
        var parent = new UIScreenControl
        {
            Position = Vector2.Zero,
            Size = new Vector2(100, 100)
        };
        var window = new UIWindow
        {
            Position = Vector2.Zero,
            Size = new Vector2(40, 40),
            Parent = parent,
            IsTitleBarEnabled = true,
            IsWindowMovable = true
        };
        window.TitleBarHeight = 10f;

        window.HandleMouseDown(new Vector2(5, 5));
        window.HandleMouseMove(new Vector2(200, 200));

        Assert.That(window.Position, Is.EqualTo(new Vector2(60, 60)));
    }

    [Test]
    public void MouseDown_Delegates_To_Children_Topmost_First()
    {
        var window = new UIWindow
        {
            Position = Vector2.Zero,
            Size = new Vector2(100, 50)
        };
        window.TitleBarHeight = 10f;
        var a = new UIScreenControl { Position = Vector2.Zero, Size = new Vector2(100, 50), ZIndex = 0 };
        var b = new UIScreenControl { Position = Vector2.Zero, Size = new Vector2(100, 50), ZIndex = 1 };
        var hit = string.Empty;
        a.OnMouseDown = _ => { hit = "a"; return true; };
        b.OnMouseDown = _ => { hit = "b"; return true; };
        window.Add(a);
        window.Add(b);

        var handled = window.HandleMouseDown(new Vector2(10, 20));

        Assert.That(handled, Is.True);
        Assert.That(hit, Is.EqualTo("b"));
    }
}
