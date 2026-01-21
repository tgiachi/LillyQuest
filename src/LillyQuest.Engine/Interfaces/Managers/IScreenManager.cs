using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Data.Input;
using LillyQuest.Engine.Interfaces.Screens;
using Silk.NET.Input;

namespace LillyQuest.Engine.Interfaces.Managers;

/// <summary>
/// Manages screen stack for input dispatch and rendering.
/// Screens contain UI entities similar to how Scenes contain game entities.
/// Supports multiple screens rendered together with focus-based input dispatch.
/// Input goes only to the focused (top) screen in the stack.
/// </summary>
public interface IScreenManager
{
    /// <summary>
    /// Gets the currently focused screen (top of the stack), or null if no screens are active.
    /// Only the focused screen receives input events.
    /// </summary>
    IScreen? FocusedScreen { get; }

    /// <summary>
    /// Gets all screens in the stack (read-only).
    /// Index 0 is bottom (rendered first), last index is top (focused, rendered last).
    /// </summary>
    IReadOnlyList<IScreen> ScreenStack { get; }

    /// <summary>
    /// Gets the root screen (compatibility). Same as bottom of stack.
    /// </summary>
    IScreen? RootScreen { get; }

    /// <summary>
    /// Sets the root screen, replacing all existing screens.
    /// Calls OnUnload on old screens and OnLoad on new screen.
    /// </summary>
    void SetRootScreen(IScreen? screen);

    /// <summary>
    /// Adds a screen to the top of the stack (becomes focused/has input).
    /// Calls OnLoad on the screen.
    /// </summary>
    void PushScreen(IScreen screen);

    /// <summary>
    /// Removes the focused screen (top of stack) from the stack.
    /// Calls OnUnload on the screen. Focus moves to screen below.
    /// </summary>
    void PopScreen();

    /// <summary>
    /// Removes a specific screen from the top of the stack if it matches.
    /// Calls OnUnload on the screen if it was at the top.
    /// Does nothing if the screen is not at the top of the stack.
    /// </summary>
    void PopScreen(IScreen screen);

    /// <summary>
    /// Removes a specific screen from the stack.
    /// Calls OnUnload on the screen if found.
    /// Useful when changing scenes to clean up associated screens.
    /// </summary>
    void RemoveScreen(IScreen screen);

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
