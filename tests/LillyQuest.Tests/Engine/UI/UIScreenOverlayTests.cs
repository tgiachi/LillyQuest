using System;
using System.Numerics;
using LillyQuest.Engine.Screens.UI;
using NUnit.Framework;
using Silk.NET.Input;

namespace LillyQuest.Tests.Engine.UI;

public class UIScreenOverlayTests
{
    [Test]
    public void Overlay_ConsumesInput_WhenControlHandles()
    {
        var overlay = new UIScreenOverlay();
        var control = new UIScreenControl
        {
            Position = Vector2.Zero,
            Size = new Vector2(10, 10),
            OnMouseDown = _ => true
        };
        overlay.Root.Add(control);

        var handled = overlay.OnMouseDown(5, 5, Array.Empty<MouseButton>());

        Assert.That(handled, Is.True);
    }

    [Test]
    public void Overlay_ForwardsMouseMove_ToActiveControl()
    {
        var overlay = new UIScreenOverlay();
        var window = new UIWindow
        {
            Position = Vector2.Zero,
            Size = new Vector2(100, 50),
            IsTitleBarEnabled = true,
            IsWindowMovable = true,
            TitleBarHeight = 10f
        };
        overlay.Root.Add(window);

        var downHandled = overlay.OnMouseDown(5, 5, Array.Empty<MouseButton>());
        var moveHandled = overlay.OnMouseMove(20, 20);

        Assert.That(downHandled, Is.True);
        Assert.That(moveHandled, Is.True);
        Assert.That(window.Position, Is.EqualTo(new Vector2(15, 15)));
    }
}
