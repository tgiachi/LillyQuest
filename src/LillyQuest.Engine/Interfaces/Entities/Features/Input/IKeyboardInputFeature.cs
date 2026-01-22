using LillyQuest.Engine.Data.Input;
using Silk.NET.Input;

namespace LillyQuest.Engine.Interfaces.Entities.Features.Input;

/// <summary>
/// Feature interface for entities that handle keyboard input.
/// </summary>
public interface IKeyboardInputFeature
{
    /// <summary>
    /// Called when one or more keys are pressed.
    /// </summary>
    void OnKeyPress(KeyModifierType modifier, IReadOnlyList<Key> keys);

    /// <summary>
    /// Called when one or more keys are released.
    /// </summary>
    void OnKeyRelease(KeyModifierType modifier, IReadOnlyList<Key> keys);

    /// <summary>
    /// Called when pressed keys repeat (after initial delay).
    /// </summary>
    void OnKeyRepeat(KeyModifierType modifier, IReadOnlyList<Key> keys);
}
