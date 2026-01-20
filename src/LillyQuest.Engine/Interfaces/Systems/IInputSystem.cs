using System.Numerics;
using Silk.NET.Input;

namespace LillyQuest.Engine.Interfaces.Systems;

/// <summary>
/// System for capturing and dispatching input events.
/// Handles keyboard, mouse, and input consumption hierarchy.
/// </summary>
public interface IInputSystem : ISystem
{
    /// <summary>
    /// Gets whether the mouse cursor is currently captured (disabled/hidden).
    /// </summary>
    bool IsMouseCaptured { get; }

    /// <summary>
    /// Gets the set of currently pressed keyboard keys.
    /// </summary>
    IReadOnlySet<Key> CurrentKeys { get; }

    /// <summary>
    /// Gets the current mouse position in screen coordinates.
    /// </summary>
    Vector2 MousePosition { get; }

    /// <summary>
    /// Gets the set of currently pressed mouse buttons.
    /// </summary>
    IReadOnlySet<MouseButton> CurrentMouseButtons { get; }

    /// <summary>
    /// Gets the current scroll wheel states.
    /// </summary>
    IReadOnlyList<ScrollWheel> ScrollWheels { get; }

    /// <summary>
    /// Captures the mouse cursor (disables and hides it).
    /// </summary>
    void CaptureMouse();

    /// <summary>
    /// Releases the mouse cursor (enables and shows it).
    /// </summary>
    void ReleaseMouse();
}
