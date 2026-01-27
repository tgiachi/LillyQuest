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
}
