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
}
