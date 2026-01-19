using System;
using System.Linq;
using System.Numerics;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Features;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Systems;
using LillyQuest.Engine.Systems.Base;

namespace LillyQuest.Engine.Systems;

/// <summary>
/// System that manages global input focus for screens.
/// Only one screen can have focus at a time, receiving keyboard and mouse input.
/// Performs hit-testing on mouse clicks to determine focus.
/// </summary>
public class InputFocusSystem : BaseSystem, IUpdateSystem
{
    private readonly ISceneManager _sceneManager;
    private Screen? _focusedScreen;

    /// <summary>
    /// Gets the currently focused screen (or null if no focus).
    /// </summary>
    public Screen? FocusedScreen => _focusedScreen;

    public InputFocusSystem(
        IGameEntityManager entityManager,
        ISceneManager sceneManager
    ) : base("Input Focus System", 5, entityManager)
    {
        _sceneManager = sceneManager;
    }

    public void Update(GameTime gameTime)
    {
        // Input handling done via Silk.NET callbacks in Bootstrap
    }

    public void FixedUpdate(GameTime gameTime)
    {
        // Not used
    }

    /// <summary>
    /// Handles mouse click for focus determination.
    /// Performs hit-test on all visible screens from top to bottom (by Order).
    /// </summary>
    public void HandleMouseClick(int x, int y)
    {
        var clickPos = new Vector2(x, y);
        var currentScene = _sceneManager.CurrentScene;
        if (currentScene == null) return;

        // Get all visible screens sorted by Order (highest first - top-most)
        var screens = currentScene.GetSceneGameEntities()
            .OfType<Screen>()
            .Where(s => s.IsVisible)
            .OrderByDescending(s => s.Order)
            .ToList();

        // Hit-test from top to bottom
        foreach (var screen in screens)
        {
            if (screen.ContainsPoint(clickPos))
            {
                SetFocus(screen);
                return;
            }
        }

        // No screen hit - clear focus
        SetFocus(null);
    }

    /// <summary>
    /// Sets the focused screen. Only fires event if focus changed.
    /// </summary>
    public void SetFocus(Screen? screen)
    {
        if (_focusedScreen == screen) return;
        _focusedScreen = screen;
        // Could fire OnFocusChanged event here if needed
    }

    /// <summary>
    /// Dispatches mouse input to the focused screen's input feature.
    /// </summary>
    public void DispatchMouseInput(int x, int y, Action<ScreenInputFeature> action)
    {
        if (_focusedScreen == null) return;

        if (_focusedScreen.TryGetFeature<ScreenInputFeature>(out var inputFeature))
        {
            if (inputFeature.IsEnabled && inputFeature.IsMouseEnabled)
            {
                action(inputFeature);
            }
        }
    }

    /// <summary>
    /// Dispatches keyboard input to the focused screen's input feature.
    /// </summary>
    public void DispatchKeyboardInput(Action<ScreenInputFeature> action)
    {
        if (_focusedScreen == null) return;

        if (_focusedScreen.TryGetFeature<ScreenInputFeature>(out var inputFeature))
        {
            if (inputFeature.IsEnabled && inputFeature.IsKeyboardEnabled)
            {
                action(inputFeature);
            }
        }
    }
}
