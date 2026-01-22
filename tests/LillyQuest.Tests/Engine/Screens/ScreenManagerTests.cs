using System.Collections.Generic;
using LillyQuest.Engine.Data.Input;
using LillyQuest.Engine.Managers.Screens;
using LillyQuest.Engine.Managers.Screens.Base;
using LillyQuest.Engine.Screens.UI;
using NUnit.Framework;
using Silk.NET.Input;

namespace LillyQuest.Tests.Engine.Screens;

public class ScreenManagerTests
{
    [Test]
    public void DispatchMouseDown_ForwardsToUnderlyingScreen_WhenOverlayDoesNotHandle()
    {
        var manager = new ScreenManager();
        var screen = new TestScreen();
        var overlay = new UIScreenOverlay();
        manager.PushScreen(screen);
        manager.PushScreen(overlay);

        var handled = manager.DispatchMouseDown(5, 5, new List<MouseButton>());

        Assert.That(handled, Is.True);
        Assert.That(screen.MouseDownCalls, Is.EqualTo(1));
    }

    [Test]
    public void DispatchKeyPress_ForwardsToUnderlyingScreen_WhenOverlayDoesNotHandle()
    {
        var manager = new ScreenManager();
        var screen = new TestScreen();
        var overlay = new UIScreenOverlay();
        manager.PushScreen(screen);
        manager.PushScreen(overlay);

        var handled = manager.DispatchKeyPress(KeyModifierType.None, new List<Key> { Key.A });

        Assert.That(handled, Is.True);
        Assert.That(screen.KeyPressCalls, Is.EqualTo(1));
    }

    private sealed class TestScreen : BaseScreen
    {
        public int MouseDownCalls { get; private set; }
        public int KeyPressCalls { get; private set; }

        public TestScreen()
        {
            Position = System.Numerics.Vector2.Zero;
            Size = new System.Numerics.Vector2(100, 100);
        }

        public override bool OnMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons)
        {
            MouseDownCalls++;
            return true;
        }

        public override bool OnKeyPress(KeyModifierType modifier, IReadOnlyList<Key> keys)
        {
            KeyPressCalls++;
            return true;
        }
    }
}
