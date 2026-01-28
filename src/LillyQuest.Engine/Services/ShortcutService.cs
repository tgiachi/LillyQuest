using LillyQuest.Core.Types;
using LillyQuest.Core.Utils;
using LillyQuest.Engine.Data.Input;
using LillyQuest.Engine.Interfaces.Services;
using LillyQuest.Engine.Types;
using LillyQuest.Engine.Utils;
using Silk.NET.Input;

namespace LillyQuest.Engine.Services;

/// <summary>
/// Provides shortcut registration and dispatch to actions.
/// Thread-safe implementation using locks.
/// </summary>
public sealed class ShortcutService : IShortcutService
{
    private readonly IActionService _actionService;
    private readonly Dictionary<ShortcutLookup, List<ShortcutBinding>> _bindingsByKey = new();
    private readonly Stack<InputContextType> _contextStack = new();
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new shortcut service.
    /// </summary>
    /// <param name="actionService">Action service dispatcher.</param>
    public ShortcutService(IActionService actionService)
    {
        _actionService = actionService;
        _contextStack.Push(InputContextType.Gameplay);
    }

    private readonly record struct ShortcutLookup(Key Key, KeyModifierType Modifier, ShortcutTriggerType Trigger);

    /// <summary>
    /// Gets the current context.
    /// </summary>
    /// <returns>Top-most context.</returns>
    public InputContextType GetCurrentContext()
    {
        lock (_lock)
        {
            return _contextStack.Count == 0 ? InputContextType.Gameplay : _contextStack.Peek();
        }
    }

    /// <summary>
    /// Handles key press events.
    /// </summary>
    /// <param name="modifier">Modifier flags.</param>
    /// <param name="keys">Pressed keys.</param>
    public void HandleKeyPress(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        HandleKeys(modifier, keys, ShortcutTriggerType.Press);
    }

    /// <summary>
    /// Handles key release events.
    /// </summary>
    /// <param name="modifier">Modifier flags.</param>
    /// <param name="keys">Released keys.</param>
    public void HandleKeyRelease(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        var currentContext = GetCurrentContext();

        // First, release any actions that were marked as in-use via Press trigger
        foreach (var key in keys)
        {
            var pressLookup = new ShortcutLookup(key, modifier, ShortcutTriggerType.Press);

            List<ShortcutBinding>? pressBindingsSnapshot;
            lock (_lock)
            {
                if (!_bindingsByKey.TryGetValue(pressLookup, out var pressBindings))
                {
                    continue;
                }
                pressBindingsSnapshot = pressBindings.ToList();
            }

            foreach (var binding in pressBindingsSnapshot)
            {
                if (binding.Context != InputContextType.Global && binding.Context != currentContext)
                {
                    continue;
                }

                _actionService.MarkActionReleased(binding.ActionName);
            }
        }

        // Then, handle any explicit Release trigger bindings
        HandleKeys(modifier, keys, ShortcutTriggerType.Release);
    }

    /// <summary>
    /// Handles key repeat events.
    /// </summary>
    /// <param name="modifier">Modifier flags.</param>
    /// <param name="keys">Repeated keys.</param>
    public void HandleKeyRepeat(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        HandleKeys(modifier, keys, ShortcutTriggerType.Repeat);
    }

    /// <summary>
    /// Pops the current context when possible.
    /// </summary>
    /// <returns>True when a context was popped.</returns>
    public bool PopContext()
    {
        lock (_lock)
        {
            if (_contextStack.Count <= 1)
            {
                return false;
            }

            _contextStack.Pop();

            return true;
        }
    }

    /// <summary>
    /// Pushes a new context on top of the stack.
    /// </summary>
    /// <param name="context">Context to push.</param>
    public void PushContext(InputContextType context)
    {
        lock (_lock)
        {
            _contextStack.Push(context);
        }
    }

    /// <summary>
    /// Registers a shortcut for an action.
    /// </summary>
    /// <param name="actionName">Action name.</param>
    /// <param name="action">Action callback.</param>
    /// <param name="context">Context scope.</param>
    /// <param name="shortcut">Shortcut string.</param>
    /// <param name="trigger">Trigger type.</param>
    /// <returns>True when registered.</returns>
    public bool RegisterShortcut(
        string actionName,
        Action action,
        InputContextType context,
        string shortcut,
        ShortcutTriggerType trigger
    )
    {
        if (string.IsNullOrWhiteSpace(actionName) || action is null)
        {
            return false;
        }

        _actionService.RegisterAction(actionName, action);

        return RegisterShortcut(actionName, context, shortcut, trigger);
    }

    /// <summary>
    /// Registers a shortcut for an action.
    /// </summary>
    /// <param name="actionName">Action name.</param>
    /// <param name="context">Context scope.</param>
    /// <param name="shortcut">Shortcut string.</param>
    /// <param name="trigger">Trigger type.</param>
    /// <returns>True when registered.</returns>
    public bool RegisterShortcut(string actionName, InputContextType context, string shortcut, ShortcutTriggerType trigger)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            return false;
        }

        if (!KeyboardUtils.TryParseShortcut(shortcut, out var modifier, out var key))
        {
            return false;
        }

        var normalizedName = NormalizeActionName(actionName);
        var binding = new ShortcutBinding(normalizedName, context, modifier, key, trigger);
        var lookup = new ShortcutLookup(key, modifier, trigger);

        lock (_lock)
        {
            if (!_bindingsByKey.TryGetValue(lookup, out var bindings))
            {
                bindings = new();
                _bindingsByKey[lookup] = bindings;
            }

            foreach (var existing in bindings)
            {
                if (existing.ActionName.Equals(normalizedName, StringComparison.OrdinalIgnoreCase) &&
                    existing.Context == context)
                {
                    return false;
                }
            }

            bindings.Add(binding);
        }

        return true;
    }

    /// <summary>
    /// Replaces the current context stack with a single context.
    /// </summary>
    /// <param name="context">Context to set.</param>
    public void SetContext(InputContextType context)
    {
        lock (_lock)
        {
            _contextStack.Clear();
            _contextStack.Push(context);
        }
    }

    /// <summary>
    /// Removes a registered shortcut for an action.
    /// </summary>
    /// <param name="actionName">Action name.</param>
    /// <param name="context">Context scope.</param>
    /// <param name="shortcut">Shortcut string.</param>
    /// <param name="trigger">Trigger type.</param>
    /// <returns>True when removed.</returns>
    public bool UnregisterShortcut(string actionName, InputContextType context, string shortcut, ShortcutTriggerType trigger)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            return false;
        }

        if (!KeyboardUtils.TryParseShortcut(shortcut, out var modifier, out var key))
        {
            return false;
        }

        var normalizedName = NormalizeActionName(actionName);
        var lookup = new ShortcutLookup(key, modifier, trigger);

        lock (_lock)
        {
            if (!_bindingsByKey.TryGetValue(lookup, out var bindings))
            {
                return false;
            }

            for (var i = bindings.Count - 1; i >= 0; i--)
            {
                var existing = bindings[i];

                if (!existing.ActionName.Equals(normalizedName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (existing.Context != context)
                {
                    continue;
                }

                bindings.RemoveAt(i);

                if (bindings.Count == 0)
                {
                    _bindingsByKey.Remove(lookup);
                }

                return true;
            }
        }

        return false;
    }

    private void HandleKeys(KeyModifierType modifier, IReadOnlyList<Key> keys, ShortcutTriggerType trigger)
    {
        var currentContext = GetCurrentContext();

        foreach (var key in keys)
        {
            var lookup = new ShortcutLookup(key, modifier, trigger);

            List<ShortcutBinding>? bindingsSnapshot;
            lock (_lock)
            {
                if (!_bindingsByKey.TryGetValue(lookup, out var bindings))
                {
                    continue;
                }
                bindingsSnapshot = bindings.ToList();
            }

            foreach (var binding in bindingsSnapshot)
            {
                if (binding.Context != InputContextType.Global && binding.Context != currentContext)
                {
                    continue;
                }

                if (trigger == ShortcutTriggerType.Press)
                {
                    _actionService.MarkActionInUse(binding.ActionName);
                }
                else if (trigger == ShortcutTriggerType.Release)
                {
                    _actionService.MarkActionReleased(binding.ActionName);
                }

                _actionService.Execute(binding.ActionName);
            }
        }
    }

    private static string NormalizeActionName(string actionName)
        => StringUtils.ToUpperSnakeCase(actionName);
}
