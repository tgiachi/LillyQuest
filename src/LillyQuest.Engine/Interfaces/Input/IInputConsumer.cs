using LillyQuest.Engine.Data.Input;
using Silk.NET.Input;

namespace LillyQuest.Engine.Interfaces.Input;

/// <summary>
/// Interface for objects that can consume input events (screens, UI components, entities).
/// Hierarchical dispatch: if an object consumes input, child objects don't receive it.
/// </summary>
public interface IInputConsumer
{
    /// <summary>
    /// Gets the name/identifier of this input consumer (for debugging).
    /// </summary>
    string ConsumerId { get; }

    /// <summary>
    /// Gets whether this consumer is currently active and should receive input.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Gets child consumers (for hierarchical dispatch).
    /// </summary>
    IReadOnlyList<IInputConsumer>? GetChildren();

    /// <summary>
    /// Performs hit-testing at the given screen coordinates.
    /// Returns true if the point is inside this consumer's bounds.
    /// </summary>
    bool HitTest(int x, int y);

    /// <summary>
    /// Called when a key is pressed.
    /// Returns true if the input was consumed (prevents further dispatch).
    /// </summary>
    bool OnKeyPress(KeyModifierType modifier, IReadOnlyList<Key> keys);

    /// <summary>
    /// Called when a key is released.
    /// Returns true if the input was consumed.
    /// </summary>
    bool OnKeyRelease(KeyModifierType modifier, IReadOnlyList<Key> keys);

    /// <summary>
    /// Called when a key repeats (after initial delay).
    /// Returns true if the input was consumed.
    /// </summary>
    bool OnKeyRepeat(KeyModifierType modifier, IReadOnlyList<Key> keys);

    /// <summary>
    /// Called when a mouse button is pressed.
    /// Returns true if the input was consumed.
    /// </summary>
    bool OnMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons);

    /// <summary>
    /// Called when the mouse moves.
    /// Returns true if the input was consumed.
    /// </summary>
    bool OnMouseMove(int x, int y);

    /// <summary>
    /// Called when a mouse button is released.
    /// Returns true if the input was consumed.
    /// </summary>
    bool OnMouseUp(int x, int y, IReadOnlyList<MouseButton> buttons);

    /// <summary>
    /// Called when the mouse wheel scrolls.
    /// Returns true if the input was consumed.
    /// </summary>
    bool OnMouseWheel(int x, int y, float delta);
}
