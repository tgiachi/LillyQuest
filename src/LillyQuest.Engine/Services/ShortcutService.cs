using DarkLilly.Core.Types;
using DarkLilly.Core.Utils;
using DarkLilly.Engine.Interfaces.Services;
using DarkLilly.Engine.Types;
using Silk.NET.Input;

namespace DarkLilly.Engine.Services;

/// <summary>
/// Provides shortcut registration and dispatch to actions.
/// </summary>
public sealed class ShortcutService : IShortcutService
{
    private readonly IActionService _actionService;
    private readonly Dictionary<ShortcutLookup, List<ShortcutBinding>> _bindingsByKey = new();
    private readonly Stack<InputContextType> _contextStack = new();

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
        => _contextStack.Count == 0 ? InputContextType.Gameplay : _contextStack.Peek();

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

            if (_bindingsByKey.TryGetValue(pressLookup, out var pressBindings))
            {
                foreach (var binding in pressBindings)
                {
                    if (binding.Context != InputContextType.Global && binding.Context != currentContext)
                    {
                        continue;
                    }

                    _actionService.MarkActionReleased(binding.ActionName);
                }
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
        if (_contextStack.Count <= 1)
        {
            return false;
        }

        _contextStack.Pop();

        return true;
    }

    /// <summary>
    /// Pushes a new context on top of the stack.
    /// </summary>
    /// <param name="context">Context to push.</param>
    public void PushContext(InputContextType context)
    {
        _contextStack.Push(context);
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

        return true;
    }

    /// <summary>
    /// Replaces the current context stack with a single context.
    /// </summary>
    /// <param name="context">Context to set.</param>
    public void SetContext(InputContextType context)
    {
        _contextStack.Clear();
        _contextStack.Push(context);
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

        return false;
    }

    private void HandleKeys(KeyModifierType modifier, IReadOnlyList<Key> keys, ShortcutTriggerType trigger)
    {
        var currentContext = GetCurrentContext();

        foreach (var key in keys)
        {
            var lookup = new ShortcutLookup(key, modifier, trigger);

            if (!_bindingsByKey.TryGetValue(lookup, out var bindings))
            {
                continue;
            }

            foreach (var binding in bindings)
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
