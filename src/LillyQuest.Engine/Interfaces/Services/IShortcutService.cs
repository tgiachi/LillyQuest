using LillyQuest.Core.Types;
using Silk.NET.Input;

namespace LillyQuest.Engine.Interfaces.Services;

/// <summary>
/// Defines shortcut registration and dispatch behavior.
/// </summary>
public interface IShortcutService
{
    /// <summary>
    /// Gets the current context.
    /// </summary>
    /// <returns>Top-most context.</returns>
    InputContextType GetCurrentContext();

    /// <summary>
    /// Handles key press events.
    /// </summary>
    /// <param name="modifier">Modifier flags.</param>
    /// <param name="keys">Pressed keys.</param>
    void HandleKeyPress(KeyModifierType modifier, IReadOnlyList<Key> keys);

    /// <summary>
    /// Handles key release events.
    /// </summary>
    /// <param name="modifier">Modifier flags.</param>
    /// <param name="keys">Released keys.</param>
    void HandleKeyRelease(KeyModifierType modifier, IReadOnlyList<Key> keys);

    /// <summary>
    /// Handles key repeat events.
    /// </summary>
    /// <param name="modifier">Modifier flags.</param>
    /// <param name="keys">Repeated keys.</param>
    void HandleKeyRepeat(KeyModifierType modifier, IReadOnlyList<Key> keys);

    /// <summary>
    /// Pops the current context when possible.
    /// </summary>
    /// <returns>True when a context was popped.</returns>
    bool PopContext();

    /// <summary>
    /// Pushes a new context on top of the stack.
    /// </summary>
    /// <param name="context">Context to push.</param>
    void PushContext(InputContextType context);

    /// <summary>
    /// Registers a shortcut for an action.
    /// </summary>
    /// <param name="actionName">Action name.</param>
    /// <param name="context">Context scope.</param>
    /// <param name="shortcut">Shortcut string.</param>
    /// <param name="trigger">Trigger type.</param>
    /// <returns>True when registered.</returns>
    bool RegisterShortcut(string actionName, InputContextType context, string shortcut, ShortcutTriggerType trigger);

    /// <summary>
    /// Replaces the current context stack with a single context.
    /// </summary>
    /// <param name="context">Context to set.</param>
    void SetContext(InputContextType context);

    /// <summary>
    /// Removes a registered shortcut for an action.
    /// </summary>
    /// <param name="actionName">Action name.</param>
    /// <param name="context">Context scope.</param>
    /// <param name="shortcut">Shortcut string.</param>
    /// <param name="trigger">Trigger type.</param>
    /// <returns>True when removed.</returns>
    bool UnregisterShortcut(string actionName, InputContextType context, string shortcut, ShortcutTriggerType trigger);
}
