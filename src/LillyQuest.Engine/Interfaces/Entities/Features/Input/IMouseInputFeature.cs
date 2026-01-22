using Silk.NET.Input;

namespace LillyQuest.Engine.Interfaces.Entities.Features.Input;

/// <summary>
/// Feature interface for entities that handle mouse input.
/// </summary>
public interface IMouseInputFeature
{
    /// <summary>
    /// Called when one or more mouse buttons are pressed.
    /// </summary>
    void OnMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons);

    /// <summary>
    /// Called when the mouse moves.
    /// </summary>
    void OnMouseMove(int x, int y);

    /// <summary>
    /// Called when one or more mouse buttons are released.
    /// </summary>
    void OnMouseUp(int x, int y, IReadOnlyList<MouseButton> buttons);

    /// <summary>
    /// Called when the mouse wheel scrolls.
    /// </summary>
    void OnMouseWheel(int x, int y, float delta);
}
