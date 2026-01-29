using LillyQuest.Core.Types;
using LillyQuest.Engine.Data.Input;
using Silk.NET.Input;

namespace LillyQuest.Engine.Types;

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
    /// Gets the trigger type (can be combined flags).
    /// </summary>
    public ShortcutTriggerType Trigger { get; }

    /// <summary>
    /// Gets the repeat delay in milliseconds (0 = no throttling).
    /// </summary>
    public int RepeatDelayMs { get; }

    /// <summary>
    /// Initializes a new shortcut binding.
    /// </summary>
    /// <param name="actionName">Action name.</param>
    /// <param name="context">Context scope.</param>
    /// <param name="modifier">Modifier flags.</param>
    /// <param name="key">Key.</param>
    /// <param name="trigger">Trigger type (can be combined flags).</param>
    /// <param name="repeatDelayMs">Repeat delay in milliseconds (0 = no throttling).</param>
    public ShortcutBinding(
        string actionName,
        InputContextType context,
        KeyModifierType modifier,
        Key key,
        ShortcutTriggerType trigger,
        int repeatDelayMs = 0
    )
    {
        ActionName = actionName;
        Context = context;
        Modifier = modifier;
        Key = key;
        Trigger = trigger;
        RepeatDelayMs = repeatDelayMs;
    }
}
