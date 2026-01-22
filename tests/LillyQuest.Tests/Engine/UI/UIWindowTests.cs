using System.Numerics;
using LillyQuest.Engine.Screens.UI;

namespace LillyQuest.Tests.Engine.UI;

public class UIWindowTests
{
    [Test]
    public void BackgroundColor_UsesAlphaMultiplier()
    {
        var window = new UIWindow
        {
            BackgroundColor = new(255, 10, 20, 30),
            BackgroundAlpha = 0.5f
        };

        var color = window.GetBackgroundColorWithAlpha();

        Assert.That(color.A, Is.EqualTo(128));
    }

    [Test]
    public void Drag_Clamps_To_Parent_Bounds()
    {
        var parent = new UIScreenControl
        {
            Position = Vector2.Zero,
            Size = new(100, 100)
        };
        var window = new UIWindow
        {
            Position = Vector2.Zero,
            Size = new(40, 40),
            Parent = parent,
            IsTitleBarEnabled = true,
            IsWindowMovable = true
        };
        window.TitleBarHeight = 10f;

        window.HandleMouseDown(new(5, 5));
        window.HandleMouseMove(new(200, 200));

        Assert.That(window.Position, Is.EqualTo(new Vector2(60, 60)));
    }

    [Test]
    public void MouseDown_Delegates_To_Children_Topmost_First()
    {
        var window = new UIWindow
        {
            Position = Vector2.Zero,
            Size = new(100, 50)
        };
        window.TitleBarHeight = 10f;
        var a = new UIScreenControl { Position = Vector2.Zero, Size = new(100, 50), ZIndex = 0 };
        var b = new UIScreenControl { Position = Vector2.Zero, Size = new(100, 50), ZIndex = 1 };
        var hit = string.Empty;
        a.OnMouseDown = _ =>
                        {
                            hit = "a";

                            return true;
                        };
        b.OnMouseDown = _ =>
                        {
                            hit = "b";

                            return true;
                        };
        window.Add(a);
        window.Add(b);

        var handled = window.HandleMouseDown(new(10, 20));

        Assert.That(handled, Is.True);
        Assert.That(hit, Is.EqualTo("b"));
    }

    [Test]
    public void MouseDown_TitleBar_StartsDrag_WhenMovable()
    {
        var window = new UIWindow
        {
            Position = Vector2.Zero,
            Size = new(100, 50),
            IsTitleBarEnabled = true,
            IsWindowMovable = true
        };
        window.TitleBarHeight = 10f;

        var handled = window.HandleMouseDown(new(5, 5));
        var moved = window.HandleMouseMove(new(20, 20));

        Assert.That(handled, Is.True);
        Assert.That(moved, Is.True);
        Assert.That(window.Position, Is.EqualTo(new Vector2(15, 15)));
    }

    [Test]
    public void TitleFontSettings_AreConfigurable()
    {
        var window = new UIWindow
        {
            TitleFontName = "alloy",
            TitleFontSize = 18
        };

        Assert.That(window.TitleFontName, Is.EqualTo("alloy"));
        Assert.That(window.TitleFontSize, Is.EqualTo(18));
    }
}
