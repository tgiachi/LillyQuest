namespace LillyQuest.Core.Types;

/// <summary>
/// Defines active input contexts for shortcut routing.
/// </summary>
public enum InputContextType
{
    /// <summary>
    /// Global context, always evaluated.
    /// </summary>
    Global = 0,

    /// <summary>
    /// Gameplay context.
    /// </summary>
    Gameplay = 1,

    /// <summary>
    /// UI context.
    /// </summary>
    UI = 2,

    /// <summary>
    /// Console context.
    /// </summary>
    Console = 3
}
