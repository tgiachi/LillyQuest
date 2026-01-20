using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Primitives;
using LillyQuest.Core.Types;
using LillyQuest.Engine.Data.Input;
using LillyQuest.Engine.Interfaces.Entities.Features.Input;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Services;
using LillyQuest.Engine.Interfaces.Systems;
using LillyQuest.Engine.Types;
using Serilog;
using Silk.NET.Input;
using Silk.NET.Input.Extensions;

namespace LillyQuest.Engine.Systems;

/// <summary>
/// System for capturing and dispatching input events.
/// Captures raw input from keyboard/mouse, tracks state changes, and dispatches to:
/// 1. ShortcutService (global shortcuts)
/// 2. ScreenManager (hierarchical UI dispatch)
/// 3. Entities with input features (game world input)
/// </summary>
public sealed class InputSystem : IInputSystem
{
    private const double KeyRepeatInitialDelay = 0.2;
    private const double KeyRepeatInterval = 0.05;

    private readonly EngineRenderContext _renderContext;
    private readonly IShortcutService _shortcutService;
    private readonly IScreenManager _screenManager;
    private readonly IGameEntityManager _entityManager;
    private readonly ILogger _logger = Log.ForContext<InputSystem>();

    // ISystem properties
    public uint Order => 0;
    public string Name => "InputSystem";
    public SystemQueryType QueryType => SystemQueryType.Updateable;

    // Keyboard state tracking
    private readonly HashSet<Key> _currentKeys = [];
    private readonly HashSet<Key> _previousKeys = [];
    private readonly List<Key> _pressedKeys = [];
    private readonly List<Key> _repeatKeys = [];
    private readonly List<Key> _releasedKeys = [];
    private readonly Dictionary<Key, double> _nextRepeatTimeByKey = new();

    // Mouse state tracking
    private Vector2 _mousePosition;
    private ScrollWheel[] _previousScrollWheels = [];
    private readonly HashSet<MouseButton> _currentMouseButtons = [];
    private readonly HashSet<MouseButton> _previousMouseButtons = [];
    private readonly List<MouseButton> _pressedMouseButtons = [];
    private readonly List<MouseButton> _releasedMouseButtons = [];

    public bool IsMouseCaptured => _renderContext.InputContext.Mice[0].Cursor.CursorMode == CursorMode.Disabled;

    public InputSystem(
        EngineRenderContext renderContext,
        IGameEntityManager entityManager,
        IShortcutService shortcutService,
        IScreenManager screenManager
    )
    {
        _renderContext = renderContext;
        _entityManager = entityManager;
        _shortcutService = shortcutService;
        _screenManager = screenManager;
        _logger.Information("InputSystem initialized");
    }

    public void CaptureMouse()
    {
        _renderContext.InputContext.Mice[0].Cursor.CursorMode = CursorMode.Disabled;
        _logger.Debug("Mouse captured");
    }

    public void Initialize()
    {
        _logger.Debug("InputSystem initialized");
    }

    public void ProcessEntities(GameTime gameTime, IGameEntityManager entityManager)
    {
        ProcessInput(gameTime);
    }

    public void ReleaseMouse()
    {
        _renderContext.InputContext.Mice[0].Cursor.CursorMode = CursorMode.Normal;
        _logger.Debug("Mouse released");
    }



    private void DispatchKeyPress(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        // 1. ShortcutService (global shortcuts)
        _shortcutService.HandleKeyPress(modifier, keys);

        // 2. ScreenManager (UI screen + screen entities)
        if (_screenManager.DispatchKeyPress(modifier, keys))
        {
            return; // UI consumed it
        }

        // 3. Game world entities with features
        foreach (var entity in _entityManager.GetQueryOf<IKeyboardInputFeature>())
        {
            entity.OnKeyPress(modifier, keys);
        }
    }

    private void DispatchKeyRelease(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        // 1. ShortcutService - convert modifier type
        _shortcutService.HandleKeyRelease(modifier, keys);

        // 2. ScreenManager
        if (_screenManager.DispatchKeyRelease(modifier, keys))
        {
            return;
        }

        // 3. Entities with features
        foreach (var entity in _entityManager.GetQueryOf<IKeyboardInputFeature>())
        {
            entity.OnKeyRelease(modifier, keys);
        }
    }

    private void DispatchKeyRepeat(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        // 1. ShortcutService - convert modifier type
        _shortcutService.HandleKeyRepeat(modifier, keys);

        // 2. ScreenManager
        if (_screenManager.DispatchKeyRepeat(modifier, keys))
        {
            return;
        }

        // 3. Entities with features
        foreach (var entity in _entityManager.GetQueryOf<IKeyboardInputFeature>())
        {
            entity.OnKeyRepeat(modifier, keys);
        }
    }

    private void DispatchMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons)
    {
        // 1. ScreenManager (hierarchical UI dispatch with hit-testing)
        if (_screenManager.DispatchMouseDown(x, y, buttons))
        {
            return;
        }

        // 2. Entities with features
        foreach (var entity in _entityManager.GetQueryOf<IMouseInputFeature>())
        {
            entity.OnMouseDown(x, y, buttons);
        }
    }

    private void DispatchMouseMove(int x, int y)
    {
        // 1. ScreenManager (hierarchical UI dispatch)
        if (_screenManager.DispatchMouseMove(x, y))
        {
            return;
        }

        // 2. Entities with features
        foreach (var entity in _entityManager.GetQueryOf<IMouseInputFeature>())
        {
            entity.OnMouseMove(x, y);
        }
    }

    private void DispatchMouseUp(int x, int y, IReadOnlyList<MouseButton> buttons)
    {
        // 1. ScreenManager
        if (_screenManager.DispatchMouseUp(x, y, buttons))
        {
            return;
        }

        // 2. Entities with features
        foreach (var entity in _entityManager.GetQueryOf<IMouseInputFeature>())
        {
            entity.OnMouseUp(x, y, buttons);
        }
    }

    private void DispatchMouseWheel(int x, int y, float delta)
    {
        // 1. ScreenManager
        if (_screenManager.DispatchMouseWheel(x, y, delta))
        {
            return;
        }

        // 2. Entities with features
        foreach (var entity in _entityManager.GetQueryOf<IMouseInputFeature>())
        {
            entity.OnMouseWheel(x, y, delta);
        }
    }

    private static KeyModifierType GetModifierFromKeys(HashSet<Key> pressedKeys)
    {
        var modifier = KeyModifierType.None;

        if (pressedKeys.Contains(Key.ShiftLeft) || pressedKeys.Contains(Key.ShiftRight))
        {
            modifier |= KeyModifierType.Shift;
        }

        if (pressedKeys.Contains(Key.ControlLeft) || pressedKeys.Contains(Key.ControlRight))
        {
            modifier |= KeyModifierType.Control;
        }

        if (pressedKeys.Contains(Key.AltLeft) || pressedKeys.Contains(Key.AltRight))
        {
            modifier |= KeyModifierType.Alt;
        }

        if (pressedKeys.Contains(Key.SuperLeft) || pressedKeys.Contains(Key.SuperRight))
        {
            modifier |= KeyModifierType.Meta;
        }

        return modifier;
    }

    private void ProcessInput(GameTime gameTime)
    {
        var keyboard = _renderContext.InputContext.Keyboards[0];
        var mouse = _renderContext.InputContext.Mice[0];
        var keyboardState = keyboard.CaptureState();
        var mouseState = mouse.CaptureState();

        var now = gameTime.TotalGameTime.TotalSeconds;

        // ==================== KEYBOARD ====================
        _currentKeys.Clear();

        foreach (var key in keyboardState.GetPressedKeys())
        {
            _currentKeys.Add(key);
        }

        // Find newly pressed keys
        _pressedKeys.Clear();

        foreach (var key in _currentKeys)
        {
            if (!_previousKeys.Contains(key))
            {
                _pressedKeys.Add(key);
                _nextRepeatTimeByKey[key] = now + KeyRepeatInitialDelay;
            }
        }

        if (_pressedKeys.Count > 0)
        {
            var modifier = GetModifierFromKeys(_currentKeys);
            DispatchKeyPress(modifier, _pressedKeys);
        }

        // Find repeating keys
        _repeatKeys.Clear();

        foreach (var key in _currentKeys)
        {
            if (_nextRepeatTimeByKey.TryGetValue(key, out var nextRepeatTime) && now >= nextRepeatTime)
            {
                _repeatKeys.Add(key);
                _nextRepeatTimeByKey[key] = now + KeyRepeatInterval;
            }
        }

        if (_repeatKeys.Count > 0)
        {
            var modifier = GetModifierFromKeys(_currentKeys);
            DispatchKeyRepeat(modifier, _repeatKeys);
        }

        // Find released keys
        _releasedKeys.Clear();

        foreach (var key in _previousKeys)
        {
            if (!_currentKeys.Contains(key))
            {
                _releasedKeys.Add(key);
                _nextRepeatTimeByKey.Remove(key);
            }
        }

        if (_releasedKeys.Count > 0)
        {
            var modifier = GetModifierFromKeys(_currentKeys);
            DispatchKeyRelease(modifier, _releasedKeys);
        }

        _previousKeys.Clear();

        foreach (var key in _currentKeys)
        {
            _previousKeys.Add(key);
        }

        // ==================== MOUSE POSITION ====================
        if (_mousePosition != mouseState.Position)
        {
            _mousePosition = mouseState.Position;
            DispatchMouseMove((int)_mousePosition.X, (int)_mousePosition.Y);
        }

        // ==================== MOUSE SCROLL ====================
        var scrollWheels = mouseState.GetScrollWheels();

        if (_previousScrollWheels.Length != scrollWheels.Length)
        {
            _previousScrollWheels = new ScrollWheel[scrollWheels.Length];

            for (var i = 0; i < scrollWheels.Length; i++)
            {
                _previousScrollWheels[i] = scrollWheels[i];
            }
        }
        else
        {
            var wheelDelta = 0.0f;

            for (var i = 0; i < scrollWheels.Length; i++)
            {
                var currentWheel = scrollWheels[i];
                var previousWheel = _previousScrollWheels[i];
                var delta = currentWheel.Y - previousWheel.Y;

                if (delta != 0.0f)
                {
                    wheelDelta += delta;
                }

                _previousScrollWheels[i] = currentWheel;
            }

            if (wheelDelta != 0.0f)
            {
                DispatchMouseWheel((int)_mousePosition.X, (int)_mousePosition.Y, wheelDelta);
            }
        }

        // ==================== MOUSE BUTTONS ====================
        _currentMouseButtons.Clear();

        foreach (var button in mouseState.GetPressedButtons())
        {
            _currentMouseButtons.Add(button);
        }

        _pressedMouseButtons.Clear();

        foreach (var button in _currentMouseButtons)
        {
            if (!_previousMouseButtons.Contains(button))
            {
                _pressedMouseButtons.Add(button);
            }
        }

        if (_pressedMouseButtons.Count > 0)
        {
            DispatchMouseDown((int)_mousePosition.X, (int)_mousePosition.Y, _pressedMouseButtons);
        }

        _releasedMouseButtons.Clear();

        foreach (var button in _previousMouseButtons)
        {
            if (!_currentMouseButtons.Contains(button))
            {
                _releasedMouseButtons.Add(button);
            }
        }

        if (_releasedMouseButtons.Count > 0)
        {
            DispatchMouseUp((int)_mousePosition.X, (int)_mousePosition.Y, _releasedMouseButtons);
        }

        _previousMouseButtons.Clear();

        foreach (var button in _currentMouseButtons)
        {
            _previousMouseButtons.Add(button);
        }
    }
}
