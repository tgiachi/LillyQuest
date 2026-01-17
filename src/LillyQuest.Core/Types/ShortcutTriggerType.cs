namespace LillyQuest.Core.Types;

/// <summary>
/// Defines when a shortcut should trigger.
/// </summary>
public enum ShortcutTriggerType
{
    /// <summary>
    /// Trigger on key press.
    /// </summary>
    Press = 0,

    /// <summary>
    /// Trigger on key repeat.
    /// </summary>
    Repeat = 1,

    /// <summary>
    /// Trigger on key release.
    /// </summary>
    Release = 2
}
