using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Data.Input;
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Input;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Screens;
using Serilog;
using Silk.NET.Input;

namespace LillyQuest.Engine.Managers.Screens;

/// <summary>
/// Manages the screen stack for input dispatch and rendering.
/// Supports multiple screens rendered together with focus-based input dispatch.
/// Only the focused (top) screen receives input events by default.
/// Screens contain UI entities that are updated and rendered like game entities.
/// </summary>
public sealed class ScreenManager : IScreenManager
{
    private readonly ILogger _logger = Log.ForContext<ScreenManager>();
    private readonly Stack<IScreen> _screenStack = [];

    public IScreen? FocusedScreen => _screenStack.Count > 0 ? _screenStack.Peek() : null;
    public IReadOnlyList<IScreen> ScreenStack => _screenStack.ToList().AsReadOnly();
    public IScreen? RootScreen => _screenStack.Count > 0 ? _screenStack.LastOrDefault() : null;

    /// <summary>
    /// Dispatches key press events to the focused screen and its entities.
    /// Only the focused (top) screen receives input.
    /// </summary>
    public bool DispatchKeyPress(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        return DispatchToScreens(
            screen =>
            {
                if (screen.OnKeyPress(modifier, keys))
                {
                    return true;
                }

                return DispatchToScreenEntities(
                    screen,
                    entity =>
                    {
                        if (entity is IInputConsumer inputConsumer)
                        {
                            return inputConsumer.OnKeyPress(modifier, keys);
                        }

                        return false;
                    }
                );
            }
        );
    }

    /// <summary>
    /// Dispatches key release events to screen and its entities.
    /// </summary>
    public bool DispatchKeyRelease(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        return DispatchToScreens(
            screen =>
            {
                if (screen.OnKeyRelease(modifier, keys))
                {
                    return true;
                }

                return DispatchToScreenEntities(
                    screen,
                    entity =>
                    {
                        if (entity is IInputConsumer inputConsumer)
                        {
                            return inputConsumer.OnKeyRelease(modifier, keys);
                        }

                        return false;
                    }
                );
            }
        );
    }

    /// <summary>
    /// Dispatches key repeat events to screen and its entities.
    /// </summary>
    public bool DispatchKeyRepeat(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        return DispatchToScreens(
            screen =>
            {
                if (screen.OnKeyRepeat(modifier, keys))
                {
                    return true;
                }

                return DispatchToScreenEntities(
                    screen,
                    entity =>
                    {
                        if (entity is IInputConsumer inputConsumer)
                        {
                            return inputConsumer.OnKeyRepeat(modifier, keys);
                        }

                        return false;
                    }
                );
            }
        );
    }

    /// <summary>
    /// Dispatches mouse down events to screen and its entities (with hit-testing).
    /// Mouse coordinates are already in logical (DPI-independent) pixels from Silk.NET.
    /// </summary>
    public bool DispatchMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons)
    {
        return DispatchToScreens(
            screen =>
            {
                if (screen.HitTest(x, y) && screen.OnMouseDown(x, y, buttons))
                {
                    return true;
                }

                var entities = screen.GetScreenGameObjects().ToList();

                for (var i = entities.Count - 1; i >= 0; i--)
                {
                    var entity = entities[i];

                    if (entity is IInputConsumer inputConsumer)
                    {
                        if (inputConsumer.HitTest(x, y) && inputConsumer.OnMouseDown(x, y, buttons))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        );
    }

    /// <summary>
    /// Dispatches mouse move events to screen and its entities.
    /// Mouse coordinates are already in logical (DPI-independent) pixels from Silk.NET.
    /// </summary>
    public bool DispatchMouseMove(int x, int y)
    {
        return DispatchToScreens(
            screen =>
            {
                if (screen.OnMouseMove(x, y))
                {
                    return true;
                }

                return DispatchToScreenEntities(
                    screen,
                    entity =>
                    {
                        if (entity is IInputConsumer inputConsumer)
                        {
                            return inputConsumer.OnMouseMove(x, y);
                        }

                        return false;
                    }
                );
            }
        );
    }

    /// <summary>
    /// Dispatches mouse up events to screen and its entities.
    /// Mouse coordinates are already in logical (DPI-independent) pixels from Silk.NET.
    /// </summary>
    public bool DispatchMouseUp(int x, int y, IReadOnlyList<MouseButton> buttons)
    {
        return DispatchToScreens(
            screen =>
            {
                if (screen.OnMouseUp(x, y, buttons))
                {
                    return true;
                }

                return DispatchToScreenEntities(
                    screen,
                    entity =>
                    {
                        if (entity is IInputConsumer inputConsumer)
                        {
                            return inputConsumer.OnMouseUp(x, y, buttons);
                        }

                        return false;
                    }
                );
            }
        );
    }

    /// <summary>
    /// Dispatches mouse wheel events to screen and its entities.
    /// Mouse coordinates are already in logical (DPI-independent) pixels from Silk.NET.
    /// </summary>
    public bool DispatchMouseWheel(int x, int y, float delta)
    {
        return DispatchToScreens(
            screen =>
            {
                if (screen.HitTest(x, y) && screen.OnMouseWheel(x, y, delta))
                {
                    return true;
                }

                var entities = screen.GetScreenGameObjects().ToList();

                for (var i = entities.Count - 1; i >= 0; i--)
                {
                    var entity = entities[i];

                    if (entity is IInputConsumer inputConsumer)
                    {
                        if (inputConsumer.HitTest(x, y) && inputConsumer.OnMouseWheel(x, y, delta))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        );
    }

    /// <summary>
    /// Removes the focused screen (top of stack).
    /// Calls OnUnload on the screen. Focus moves to screen below.
    /// </summary>
    public void PopScreen()
    {
        if (_screenStack.Count == 0)
        {
            _logger.Warning("Attempted to pop screen from empty stack");

            return;
        }

        var screen = _screenStack.Pop();
        screen.OnUnload();
        _logger.Debug("Popped screen {ScreenId}, stack size: {StackSize}", screen.ConsumerId, _screenStack.Count);
    }

    /// <summary>
    /// Removes a specific screen from the top of the stack if it matches.
    /// Calls OnUnload on the screen if it was at the top.
    /// </summary>
    public void PopScreen(IScreen screen)
    {
        if (_screenStack.Count == 0)
        {
            _logger.Warning("Attempted to pop screen from empty stack");

            return;
        }

        var focusedScreen = _screenStack.Peek();

        if (focusedScreen == screen)
        {
            _screenStack.Pop();
            screen.OnUnload();
            _logger.Debug("Popped screen {ScreenId}, stack size: {StackSize}", screen.ConsumerId, _screenStack.Count);
        }
        else
        {
            _logger.Warning("Cannot pop screen {ScreenId}: not at top of stack", screen.ConsumerId);
        }
    }

    /// <summary>
    /// Adds a screen to the top of the stack (becomes focused).
    /// Calls OnLoad on the screen.
    /// </summary>
    public void PushScreen(IScreen screen)
    {
        _screenStack.Push(screen);
        screen.OnLoad();
        _logger.Debug("Pushed screen {ScreenId}, stack size: {StackSize}", screen.ConsumerId, _screenStack.Count);
    }

    /// <summary>
    /// Removes a specific screen from the stack.
    /// Calls OnUnload on the screen if found.
    /// </summary>
    public void RemoveScreen(IScreen screen)
    {
        var list = _screenStack.ToList();

        if (!list.Contains(screen))
        {
            _logger.Warning("Screen {ScreenId} not found in stack", screen.ConsumerId);

            return;
        }

        _screenStack.Clear();

        foreach (var s in list)
        {
            if (s != screen)
            {
                _screenStack.Push(s);
            }
            else
            {
                s.OnUnload();
                _logger.Debug("Removed screen {ScreenId} from stack", screen.ConsumerId);
            }
        }

        // Re-push remaining screens in correct order
        var tempStack = _screenStack.ToList();
        _screenStack.Clear();

        for (var i = tempStack.Count - 1; i >= 0; i--)
        {
            _screenStack.Push(tempStack[i]);
        }
    }

    /// <summary>
    /// Renders all screens in the stack (bottom to top).
    /// </summary>
    public void Render(SpriteBatch spriteBatch, EngineRenderContext renderContext)
    {
        if (_screenStack.Count == 0)
        {
            return;
        }

        // Render screens from bottom to top (reverse order)
        var screensToRender = _screenStack.Reverse().ToList();

        foreach (var screen in screensToRender)
        {
            if (screen.IsActive)
            {
                screen.Render(spriteBatch, renderContext);
            }
        }
    }

    /// <summary>
    /// Sets the root screen, replacing all screens in the stack.
    /// Calls OnUnload on all existing screens and OnLoad on the new screen.
    /// </summary>
    public void SetRootScreen(IScreen? screen)
    {
        // Unload all screens
        while (_screenStack.Count > 0)
        {
            var oldScreen = _screenStack.Pop();
            oldScreen.OnUnload();
        }

        if (screen != null)
        {
            _screenStack.Push(screen);
            screen.OnLoad();
            _logger.Information("Set root screen to {ScreenId}", screen.ConsumerId);
        }
        else
        {
            _logger.Information("Cleared all screens");
        }
    }

    /// <summary>
    /// Updates all screens in the stack.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        if (_screenStack.Count == 0)
        {
            return;
        }

        // Update all screens
        foreach (var screen in _screenStack)
        {
            if (screen.IsActive)
            {
                screen.Update(gameTime);
            }
        }
    }


    /// <summary>
    /// Dispatches an input action to all screen entities.
    /// </summary>
    private bool DispatchToScreenEntities(
        IScreen screen,
        Func<IGameEntity, bool> dispatchAction
    )
    {
        var entities = screen.GetScreenGameObjects().ToList();

        // Try from top to bottom (last entity is topmost)
        for (var i = entities.Count - 1; i >= 0; i--)
        {
            if (dispatchAction(entities[i]))
            {
                return true; // Entity consumed input
            }
        }

        return false;
    }

    private bool DispatchToScreens(Func<IScreen, bool> dispatchAction)
    {
        foreach (var screen in _screenStack)
        {
            if (!screen.IsActive)
            {
                continue;
            }

            if (dispatchAction(screen))
            {
                return true;
            }
        }

        return false;
    }
}
