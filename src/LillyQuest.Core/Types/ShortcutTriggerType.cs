namespace LillyQuest.Core.Types;

/// <summary>
/// Defines when a shortcut should trigger. Can be combined as flags.
/// </summary>
[Flags]
public enum ShortcutTriggerType
{
    /// <summary>
    /// No trigger.
    /// </summary>
    None = 0,

    /// <summary>
    /// Trigger on key press.
    /// </summary>
    Press = 1,

    /// <summary>
    /// Trigger on key release.
    /// </summary>
    Release = 2,

    /// <summary>
    /// Trigger on key repeat (while held).
    /// </summary>
    Repeat = 4
}
