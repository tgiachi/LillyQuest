using DarkLilly.Core.Types;
using Silk.NET.Input;

namespace DarkLilly.Engine.Types;

/// <summary>
/// Represents a resolved shortcut binding for an action.
/// </summary>
public sealed class ShortcutBinding
{
    /// <summary>
    /// Gets the action name.
    /// </summary>
    public string ActionName { get; }

    /// <summary>
    /// Gets the context scope.
    /// </summary>
    public InputContextType Context { get; }

    /// <summary>
    /// Gets the modifier flags.
    /// </summary>
    public KeyModifierType Modifier { get; }

    /// <summary>
    /// Gets the key.
    /// </summary>
    public Key Key { get; }

    /// <summary>
    /// Gets the trigger type.
    /// </summary>
    public ShortcutTriggerType Trigger { get; }

    /// <summary>
    /// Initializes a new shortcut binding.
    /// </summary>
    /// <param name="actionName">Action name.</param>
    /// <param name="context">Context scope.</param>
    /// <param name="modifier">Modifier flags.</param>
    /// <param name="key">Key.</param>
    /// <param name="trigger">Trigger type.</param>
    public ShortcutBinding(
        string actionName,
        InputContextType context,
        KeyModifierType modifier,
        Key key,
        ShortcutTriggerType trigger
    )
    {
        ActionName = actionName;
        Context = context;
        Modifier = modifier;
        Key = key;
        Trigger = trigger;
    }
}
