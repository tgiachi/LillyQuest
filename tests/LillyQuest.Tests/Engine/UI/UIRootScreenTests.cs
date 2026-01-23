using System.Numerics;
using LillyQuest.Engine.Screens.UI;
using Silk.NET.Input;

namespace LillyQuest.Tests.Engine.UI;

public class UIRootScreenTests
{
    [Test]
    public void MouseDown_Captures_Control_And_Forwards_Move_Up()
    {
        var root = new UIRootScreen();
        var control = new UIScreenControl
        {
            Position = Vector2.Zero,
            Size = new(20, 20)
        };
        var moves = 0;
        var ups = 0;

        control.OnMouseDown = _ => true;
        control.OnMouseMove = _ =>
                              {
                                  moves++;

                                  return true;
                              };
        control.OnMouseUp = _ =>
                            {
                                ups++;

                                return true;
                            };
        root.Root.Add(control);

        Assert.That(root.OnMouseDown(5, 5, Array.Empty<MouseButton>()), Is.True);
        Assert.That(root.OnMouseMove(10, 10), Is.True);
        Assert.That(root.OnMouseUp(10, 10, Array.Empty<MouseButton>()), Is.True);
        Assert.That(moves, Is.EqualTo(1));
        Assert.That(ups, Is.EqualTo(1));
    }

    [Test]
    public void Modal_Window_Blocks_Input_To_Other_Controls()
    {
        var root = new UIRootScreen();
        var modal = new UIWindow { Position = Vector2.Zero, Size = new(50, 50), IsModal = true };
        var modalHit = false;
        modal.OnMouseDown = _ =>
                            {
                                modalHit = true;
                                return true;
                            };
        var other = new UIScreenControl { Position = Vector2.Zero, Size = new(100, 100) };
        var otherHit = false;
        other.OnMouseDown = _ =>
                            {
                                otherHit = true;
                                return true;
                            };

        root.Root.Add(other);
        root.Root.Add(modal);

        var handled = root.OnMouseDown(10, 10, Array.Empty<MouseButton>());

        Assert.That(handled, Is.True);
        Assert.That(modalHit, Is.True);
        Assert.That(otherHit, Is.False);
    }

    [Test]
    public void Modal_Adds_Background_Overlay()
    {
        var root = new UIRootScreen();
        var modal = new UIWindow { Position = Vector2.Zero, Size = new(50, 50), IsModal = true };

        root.Root.Add(modal);

        root.OnMouseDown(10, 10, Array.Empty<MouseButton>());

        Assert.That(root.Root.Children.Any(c => c is UIBackgroundControl), Is.True);
    }

    [Test]
    public void MouseDown_Focuses_Child_Control_When_Clicked()
    {
        var root = new UIRootScreen();
        var window = new UIWindow { Position = Vector2.Zero, Size = new(100, 100) };
        var child = new UIScreenControl
        {
            Position = new Vector2(10, 10),
            Size = new Vector2(20, 20),
            IsFocusable = true
        };
        child.OnMouseDown = _ => true;

        window.Add(child);
        root.Root.Add(window);

        root.OnMouseDown(15, 15, Array.Empty<MouseButton>());

        Assert.That(root.Root.FocusManager.Focused, Is.EqualTo(child));
    }
}
