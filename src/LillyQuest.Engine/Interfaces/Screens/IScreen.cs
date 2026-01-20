using System.Numerics;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Input;

namespace LillyQuest.Engine.Interfaces.Managers;

/// <summary>
/// Represents a screen in the game UI hierarchy.
/// Screens have position, size, and can consume input events.
/// </summary>
public interface IScreen : IInputConsumer
{
    /// <summary>
    /// Gets the screen's position on the viewport (top-left).
    /// </summary>
    Vector2 Position { get; }

    /// <summary>
    /// Gets the screen's size (width, height).
    /// </summary>
    Vector2 Size { get; }

    /// <summary>
    /// Gets whether this screen is modal (blocks input to screens below it).
    /// </summary>
    bool IsModal { get; }

    /// <summary>
    /// Called when the screen is shown/brought to focus.
    /// </summary>
    void OnShow();

    /// <summary>
    /// Called when the screen is hidden/loses focus.
    /// </summary>
    void OnHide();

    /// <summary>
    /// Called to update the screen (once per frame).
    /// </summary>
    void Update(GameTime gameTime);

    /// <summary>
    /// Called to render the screen.
    /// </summary>
    void Render();
}
