namespace LillyQuest.Engine.Data.Input;

/// <summary>
/// Keyboard modifier flags (Shift, Ctrl, Alt, Meta).
/// Can be combined with bitwise OR.
/// </summary>
[Flags]
public enum KeyModifierType
{
    /// <summary>No modifiers pressed.</summary>
    None = 0,

    /// <summary>Shift key (left or right).</summary>
    Shift = 1,

    /// <summary>Control key (left or right).</summary>
    Control = 2,

    /// <summary>Alt key (left or right).</summary>
    Alt = 4,

    /// <summary>Meta/Super key (left or right, typically Windows/Command).</summary>
    Meta = 8
}
