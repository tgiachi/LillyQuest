using System.Numerics;
using LillyQuest.Engine.Data.Input;
using LillyQuest.Engine.Managers.Screens;
using LillyQuest.Engine.Managers.Screens.Base;
using Silk.NET.Input;

namespace LillyQuest.Tests.Engine.Screens;

public class ScreenManagerTests
{
    private sealed class TestScreen : BaseScreen
    {
        public int MouseDownCalls { get; private set; }
        public int KeyPressCalls { get; private set; }
        public bool HandlesMouseDown { get; init; }
        public bool HandlesKeyPress { get; init; }

        public TestScreen()
        {
            Position = Vector2.Zero;
            Size = new(100, 100);
        }

        public override bool OnKeyPress(KeyModifierType modifier, IReadOnlyList<Key> keys)
        {
            KeyPressCalls++;

            return HandlesKeyPress;
        }

        public override bool OnMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons)
        {
            MouseDownCalls++;

            return HandlesMouseDown;
        }
    }

    [Test]
    public void DispatchKeyPress_ForwardsToUnderlyingScreen_WhenTopDoesNotHandle()
    {
        var manager = new ScreenManager();
        var bottom = new TestScreen { HandlesKeyPress = true };
        var top = new TestScreen { HandlesKeyPress = false };
        manager.PushScreen(bottom);
        manager.PushScreen(top);

        var handled = manager.DispatchKeyPress(KeyModifierType.None, new List<Key> { Key.A });

        Assert.That(handled, Is.True);
        Assert.That(bottom.KeyPressCalls, Is.EqualTo(1));
        Assert.That(top.KeyPressCalls, Is.EqualTo(1));
    }

    [Test]
    public void DispatchMouseDown_ForwardsToUnderlyingScreen_WhenTopDoesNotHandle()
    {
        var manager = new ScreenManager();
        var bottom = new TestScreen { HandlesMouseDown = true };
        var top = new TestScreen { HandlesMouseDown = false };
        manager.PushScreen(bottom);
        manager.PushScreen(top);

        var handled = manager.DispatchMouseDown(5, 5, new List<MouseButton>());

        Assert.That(handled, Is.True);
        Assert.That(bottom.MouseDownCalls, Is.EqualTo(1));
        Assert.That(top.MouseDownCalls, Is.EqualTo(1));
    }
}
