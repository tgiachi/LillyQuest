using System.Numerics;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Engine.Screens.UI;

namespace LillyQuest.Tests.Engine.UI;

public class UIWindowTests
{
    [Test]
    public void Add_Uses_Base_Children()
    {
        var window = new UIWindow();
        var child = new UIScreenControl();

        window.Add(child);

        Assert.That(window.Children.Count, Is.EqualTo(1));
        Assert.That(window.Children[0], Is.EqualTo(child));
    }

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
    public void MouseMove_Forwards_To_Active_Child()
    {
        var window = new UIWindow
        {
            Position = Vector2.Zero,
            Size = new(100, 50)
        };
        var child = new UIScreenControl
        {
            Position = Vector2.Zero,
            Size = new(100, 50)
        };
        var moves = 0;
        child.OnMouseDown = _ => true;
        child.OnMouseMove = _ =>
                            {
                                moves++;

                                return true;
                            };
        window.Add(child);

        window.HandleMouseDown(new(10, 10));
        window.HandleMouseMove(new(12, 12));

        Assert.That(moves, Is.EqualTo(1));
    }

    [Test]
    public void MouseUp_Forwards_To_Active_Child()
    {
        var window = new UIWindow
        {
            Position = Vector2.Zero,
            Size = new(100, 50)
        };
        var child = new UIScreenControl
        {
            Position = Vector2.Zero,
            Size = new(100, 50)
        };
        var ups = 0;
        child.OnMouseDown = _ => true;
        child.OnMouseUp = _ =>
                          {
                              ups++;

                              return true;
                          };
        window.Add(child);

        window.HandleMouseDown(new(10, 10));
        window.HandleMouseUp(new(12, 12));

        Assert.That(ups, Is.EqualTo(1));
    }

    [Test]
    public void Resize_Clamps_To_Min_And_Max_Size()
    {
        var window = new UIWindow
        {
            Position = Vector2.Zero,
            Size = new(100, 50),
            IsResizable = true,
            ResizeHandleSize = new(10, 10),
            MinSize = new(80, 40),
            MaxSize = new(120, 60)
        };

        var start = new Vector2(window.Position.X + window.Size.X - 1, window.Position.Y + window.Size.Y - 1);

        window.HandleMouseDown(start);
        window.HandleMouseMove(new(200, 200));
        window.HandleMouseUp(new(200, 200));

        Assert.That(window.Size, Is.EqualTo(new Vector2(120, 60)));

        start = new(window.Position.X + window.Size.X - 1, window.Position.Y + window.Size.Y - 1);

        window.HandleMouseDown(start);
        window.HandleMouseMove(new(-200, -200));
        window.HandleMouseUp(new(-200, -200));

        Assert.That(window.Size, Is.EqualTo(new Vector2(80, 40)));
    }

    [Test]
    public void Resize_Is_Ignored_When_Disabled()
    {
        var window = new UIWindow
        {
            Position = new(10, 10),
            Size = new(100, 50),
            IsResizable = false,
            ResizeHandleSize = new(10, 10)
        };

        var start = new Vector2(10 + 100 - 1, 10 + 50 - 1);

        var pressed = window.HandleMouseDown(start);
        window.HandleMouseMove(new(200, 200));
        window.HandleMouseUp(new(200, 200));

        Assert.That(pressed, Is.False);
        Assert.That(window.Size, Is.EqualTo(new Vector2(100, 50)));
    }

    [Test]
    public void ResizeHandle_Drag_Resizes_Window()
    {
        var window = new UIWindow
        {
            Position = new(10, 10),
            Size = new(100, 50),
            IsResizable = true,
            ResizeHandleSize = new(10, 10)
        };

        var start = new Vector2(10 + 100 - 1, 10 + 50 - 1);
        var moved = new Vector2(10 + 130, 10 + 80);

        var pressed = window.HandleMouseDown(start);
        var drag = window.HandleMouseMove(moved);
        var released = window.HandleMouseUp(moved);

        Assert.That(pressed, Is.True);
        Assert.That(drag, Is.True);
        Assert.That(released, Is.True);
        Assert.That(window.Size, Is.EqualTo(new Vector2(130, 80)));
    }

    [Test]
    public void TitleFontSettings_AreConfigurable()
    {
        var window = new UIWindow
        {
            TitleFont = new("alloy", 18, FontKind.TrueType)
        };

        Assert.That(window.TitleFont, Is.EqualTo(new FontRef("alloy", 18, FontKind.TrueType)));
    }
}
