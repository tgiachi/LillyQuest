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
    /// Captures the mouse cursor (disables and hides it).
    /// </summary>
    void CaptureMouse();

    /// <summary>
    /// Releases the mouse cursor (enables and shows it).
    /// </summary>
    void ReleaseMouse();
}
