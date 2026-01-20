using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Data.Input;
using LillyQuest.Engine.Interfaces.Screens;
using Silk.NET.Input;

namespace LillyQuest.Engine.Interfaces.Managers;

/// <summary>
/// Manages screen hierarchy for input dispatch and rendering.
/// Screens contain UI entities similar to how Scenes contain game entities.
/// Handles hit-testing, focus, and hierarchical input consumption.
/// </summary>
public interface IScreenManager
{
    /// <summary>
    /// Gets the root screen, or null if no screens are active.
    /// </summary>
    IScreen? RootScreen { get; }

    /// <summary>
    /// Sets the root screen of the hierarchy.
    /// Calls OnUnload on old screen and OnLoad on new screen.
    /// </summary>
    void SetRootScreen(IScreen? screen);

    /// <summary>
    /// Updates the current screen and all its entities.
    /// </summary>
    void Update(GameTime gameTime);

    /// <summary>
    /// Renders the current screen and all its entities.
    /// </summary>
    void Render(SpriteBatch spriteBatch, EngineRenderContext renderContext);

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
