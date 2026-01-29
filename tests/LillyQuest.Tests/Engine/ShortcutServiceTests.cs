using LillyQuest.Core.Types;
using LillyQuest.Core.Utils;
using LillyQuest.Engine.Data.Input;
using LillyQuest.Engine.Interfaces.Services;
using LillyQuest.Engine.Services;
using LillyQuest.Engine.Types;
using Silk.NET.Input;

namespace LillyQuest.Tests.Engine;

public class ShortcutServiceTests
{
    private sealed class StubActionService : IActionService
    {
        private readonly Dictionary<string, Action> _actions = new(StringComparer.OrdinalIgnoreCase);

        public bool Execute(string actionName)
        {
            if (string.IsNullOrWhiteSpace(actionName))
            {
                return false;
            }

            var normalizedName = NormalizeActionName(actionName);

            if (!_actions.TryGetValue(normalizedName, out var action))
            {
                return false;
            }

            action();
            return true;
        }

        public bool HasAction(string actionName)
            => !string.IsNullOrWhiteSpace(actionName) && _actions.ContainsKey(NormalizeActionName(actionName));

        public bool IsActionInUse(string actionName)
            => false;

        public void MarkActionInUse(string actionName) { }

        public void MarkActionReleased(string actionName) { }

        public void RegisterAction(string actionName, Action action)
        {
            var normalizedName = NormalizeActionName(actionName);
            _actions[normalizedName] = action;
        }

        public bool UnregisterAction(string actionName)
            => !string.IsNullOrWhiteSpace(actionName) && _actions.Remove(NormalizeActionName(actionName));

        private static string NormalizeActionName(string actionName)
            => StringUtils.ToUpperSnakeCase(actionName);
    }

    [Test]
    public void RegisterShortcut_WithAction_RegistersActionAndExecutes()
    {
        var actionService = new StubActionService();
        var shortcutService = new ShortcutService(actionService);
        var executed = false;

        var registered = shortcutService.RegisterShortcut(
            "jump",
            () => executed = true,
            InputContextType.Gameplay,
            "w",
            ShortcutTriggerType.Press
        );

        Assert.That(registered, Is.True);
        Assert.That(actionService.HasAction("jump"), Is.True);

        shortcutService.HandleKeyPress(KeyModifierType.None, new[] { Key.W });

        Assert.That(executed, Is.True);
    }

    [Test]
    public void RegisterShortcut_WithFlagsTrigger_TriggersOnBothPressAndRelease()
    {
        var actionService = new StubActionService();
        var shortcutService = new ShortcutService(actionService);
        var pressCount = 0;
        var releaseCount = 0;

        shortcutService.RegisterShortcut(
            "move",
            () => { if (pressCount == 0) pressCount++; else releaseCount++; },
            InputContextType.Gameplay,
            "w",
            ShortcutTriggerType.Press | ShortcutTriggerType.Release
        );

        shortcutService.HandleKeyPress(KeyModifierType.None, new[] { Key.W });
        Assert.That(pressCount, Is.EqualTo(1));

        shortcutService.HandleKeyRelease(KeyModifierType.None, new[] { Key.W });
        Assert.That(releaseCount, Is.EqualTo(1));
    }

    [Test]
    public void RegisterShortcut_WithRepeatDelay_ThrottlesRepeatEvents()
    {
        var actionService = new StubActionService();
        var shortcutService = new ShortcutService(actionService);
        var executeCount = 0;

        shortcutService.RegisterShortcut(
            "move",
            () => executeCount++,
            InputContextType.Gameplay,
            "w",
            ShortcutTriggerType.Repeat,
            repeatDelayMs: 100
        );

        // First repeat should execute
        shortcutService.HandleKeyRepeat(KeyModifierType.None, new[] { Key.W });
        Assert.That(executeCount, Is.EqualTo(1));

        // Immediate repeat should be throttled
        shortcutService.HandleKeyRepeat(KeyModifierType.None, new[] { Key.W });
        Assert.That(executeCount, Is.EqualTo(1)); // Still 1, throttled

        // After delay, should execute again
        Thread.Sleep(110);
        shortcutService.HandleKeyRepeat(KeyModifierType.None, new[] { Key.W });
        Assert.That(executeCount, Is.EqualTo(2));
    }

    [Test]
    public void RegisterShortcut_WithZeroRepeatDelay_DoesNotThrottle()
    {
        var actionService = new StubActionService();
        var shortcutService = new ShortcutService(actionService);
        var executeCount = 0;

        shortcutService.RegisterShortcut(
            "move",
            () => executeCount++,
            InputContextType.Gameplay,
            "w",
            ShortcutTriggerType.Repeat,
            repeatDelayMs: 0
        );

        shortcutService.HandleKeyRepeat(KeyModifierType.None, new[] { Key.W });
        shortcutService.HandleKeyRepeat(KeyModifierType.None, new[] { Key.W });
        shortcutService.HandleKeyRepeat(KeyModifierType.None, new[] { Key.W });

        Assert.That(executeCount, Is.EqualTo(3));
    }

    [Test]
    public void RegisterShortcut_WithPressAndRepeat_BothTrigger()
    {
        var actionService = new StubActionService();
        var shortcutService = new ShortcutService(actionService);
        var executeCount = 0;

        shortcutService.RegisterShortcut(
            "move",
            () => executeCount++,
            InputContextType.Gameplay,
            "w",
            ShortcutTriggerType.Press | ShortcutTriggerType.Repeat
        );

        shortcutService.HandleKeyPress(KeyModifierType.None, new[] { Key.W });
        Assert.That(executeCount, Is.EqualTo(1));

        shortcutService.HandleKeyRepeat(KeyModifierType.None, new[] { Key.W });
        Assert.That(executeCount, Is.EqualTo(2));
    }

    [Test]
    public void HandleKeyRelease_ResetsRepeatThrottle()
    {
        var actionService = new StubActionService();
        var shortcutService = new ShortcutService(actionService);
        var executeCount = 0;

        shortcutService.RegisterShortcut(
            "move",
            () => executeCount++,
            InputContextType.Gameplay,
            "w",
            ShortcutTriggerType.Repeat,
            repeatDelayMs: 1000  // Long delay
        );

        shortcutService.HandleKeyRepeat(KeyModifierType.None, new[] { Key.W });
        Assert.That(executeCount, Is.EqualTo(1));

        // Release the key - should reset throttle
        shortcutService.HandleKeyRelease(KeyModifierType.None, new[] { Key.W });

        // New press+repeat should execute immediately
        shortcutService.HandleKeyRepeat(KeyModifierType.None, new[] { Key.W });
        Assert.That(executeCount, Is.EqualTo(2));
    }
}
