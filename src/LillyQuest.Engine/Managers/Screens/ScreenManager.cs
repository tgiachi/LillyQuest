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
/// Manages the screen hierarchy for input dispatch and rendering.
/// Implements hierarchical input consumption (modal screens block input to screens below).
/// Screens contain UI entities that are updated and rendered like game entities.
/// </summary>
public sealed class ScreenManager : IScreenManager
{
    private readonly ILogger _logger = Log.ForContext<ScreenManager>();
    private readonly SpriteBatch _spriteBatch;

    public IScreen? RootScreen { get; private set; }

    public ScreenManager(SpriteBatch spriteBatch)
        => _spriteBatch = spriteBatch;

    /// <summary>
    /// Dispatches key press events to screen and its entities.
    /// </summary>
    public bool DispatchKeyPress(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        if (RootScreen is not { IsActive: true })
        {
            return false;
        }

        // Try screen itself
        if (RootScreen.OnKeyPress(modifier, keys))
        {
            return true;
        }

        // Try screen entities (UI components)
        return DispatchToScreenEntities(
            RootScreen,
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

    /// <summary>
    /// Dispatches key release events to screen and its entities.
    /// </summary>
    public bool DispatchKeyRelease(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        if (RootScreen is not { IsActive: true })
        {
            return false;
        }

        if (RootScreen.OnKeyRelease(modifier, keys))
        {
            return true;
        }

        return DispatchToScreenEntities(
            RootScreen,
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

    /// <summary>
    /// Dispatches key repeat events to screen and its entities.
    /// </summary>
    public bool DispatchKeyRepeat(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        if (RootScreen == null || !RootScreen.IsActive)
        {
            return false;
        }

        if (RootScreen.OnKeyRepeat(modifier, keys))
        {
            return true;
        }

        return DispatchToScreenEntities(
            RootScreen,
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

    /// <summary>
    /// Dispatches mouse down events to screen and its entities (with hit-testing).
    /// </summary>
    public bool DispatchMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons)
    {
        if (RootScreen == null || !RootScreen.IsActive)
        {
            return false;
        }

        // Hit-test screen first
        if (RootScreen.HitTest(x, y) && RootScreen.OnMouseDown(x, y, buttons))
        {
            return true;
        }

        // Hit-test entities (top to bottom)
        var entities = RootScreen.GetScreenGameObjects().ToList();

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

    /// <summary>
    /// Dispatches mouse move events to screen and its entities.
    /// </summary>
    public bool DispatchMouseMove(int x, int y)
    {
        if (RootScreen is not { IsActive: true })
        {
            return false;
        }

        if (RootScreen.OnMouseMove(x, y))
        {
            return true;
        }

        return DispatchToScreenEntities(
            RootScreen,
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

    /// <summary>
    /// Dispatches mouse up events to screen and its entities.
    /// </summary>
    public bool DispatchMouseUp(int x, int y, IReadOnlyList<MouseButton> buttons)
    {
        if (RootScreen == null || !RootScreen.IsActive)
        {
            return false;
        }

        if (RootScreen.OnMouseUp(x, y, buttons))
        {
            return true;
        }

        return DispatchToScreenEntities(
            RootScreen,
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

    /// <summary>
    /// Dispatches mouse wheel events to screen and its entities.
    /// </summary>
    public bool DispatchMouseWheel(int x, int y, float delta)
    {
        if (RootScreen == null || !RootScreen.IsActive)
        {
            return false;
        }

        // Hit-test screen first
        if (RootScreen.HitTest(x, y) && RootScreen.OnMouseWheel(x, y, delta))
        {
            return true;
        }

        // Try entities (top to bottom)
        var entities = RootScreen.GetScreenGameObjects().ToList();

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

    /// <summary>
    /// Renders the current screen and its entities.
    /// </summary>
    public void Render(SpriteBatch spriteBatch, EngineRenderContext renderContext)
    {
        if (RootScreen is not { IsActive: true })
        {
            return;
        }

        RootScreen.Render(spriteBatch, renderContext);
    }

    /// <summary>
    /// Sets the root screen of the hierarchy.
    /// </summary>
    public void SetRootScreen(IScreen? screen)
    {
        if (RootScreen != screen)
        {
            RootScreen?.OnUnload();
            RootScreen = screen;

            if (screen != null)
            {
                screen.OnLoad();
                _logger.Information("Set root screen to {ScreenId}", screen.ConsumerId);
            }
            else
            {
                _logger.Information("Cleared root screen");
            }
        }
    }

    /// <summary>
    /// Updates the current screen and its entities.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        if (RootScreen is not { IsActive: true })
        {
            return;
        }

        RootScreen.Update(gameTime);
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
}
