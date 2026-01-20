using LillyQuest.Engine.Data.Input;
using Silk.NET.Input;

namespace LillyQuest.Engine.Interfaces.Managers;

/// <summary>
/// Manages screen hierarchy for input dispatch and rendering.
/// Handles hit-testing, focus, and hierarchical input consumption.
/// </summary>
public interface IScreenManager
{
    /// <summary>
    /// Gets the root screen, or null if no screens are active.
    /// </summary>
    IScreen? RootScreen { get; }

    /// <summary>
    /// Performs hierarchical input dispatch to the screen tree.
    /// Hit-tests and dispatches to topmost active screen first.
    /// If a screen consumes input, stops dispatch (doesn't propagate to children).
    /// </summary>
    /// <param name="x">Mouse X coordinate.</param>
    /// <param name="y">Mouse Y coordinate.</param>
    /// <returns>True if input was consumed by any screen in the hierarchy.</returns>
    bool DispatchKeyPress(KeyModifierType modifier, IReadOnlyList<Key> keys);

    /// <summary>
    /// Dispatches key repeat event to active screens.
    /// </summary>
    bool DispatchKeyRepeat(KeyModifierType modifier, IReadOnlyList<Key> keys);

    /// <summary>
    /// Dispatches key release event to active screens.
    /// </summary>
    bool DispatchKeyRelease(KeyModifierType modifier, IReadOnlyList<Key> keys);

    /// <summary>
    /// Dispatches mouse move event to active screens.
    /// </summary>
    bool DispatchMouseMove(int x, int y);

    /// <summary>
    /// Dispatches mouse down event to active screens (with hit-testing).
    /// </summary>
    bool DispatchMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons);

    /// <summary>
    /// Dispatches mouse up event to active screens.
    /// </summary>
    bool DispatchMouseUp(int x, int y, IReadOnlyList<MouseButton> buttons);

    /// <summary>
    /// Dispatches mouse wheel event to active screens.
    /// </summary>
    bool DispatchMouseWheel(int x, int y, float delta);
}
