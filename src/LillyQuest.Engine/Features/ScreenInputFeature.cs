using System;
using System.Collections.Generic;
using System.Numerics;
using LillyQuest.Core.Types;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.Features.Input;
using Silk.NET.Input;

namespace LillyQuest.Engine.Features;

/// <summary>
/// Input feature for screens that transforms world coordinates to local coordinates
/// and provides events with transformed coordinates.
/// Only receives input when screen has focus.
/// </summary>
public class ScreenInputFeature : IMouseInputFeature, IKeyboardInputFeature
{
    private readonly Screen _screen;

    public bool IsEnabled { get; set; } = true;
    public bool IsMouseEnabled { get; set; } = true;
    public bool IsKeyboardEnabled { get; set; } = true;

    // Events with local coordinates
    public event Action<Vector2, IReadOnlyList<MouseButton>>? OnLocalMouseDown;
    public event Action<Vector2>? OnLocalMouseMove;
    public event Action<Vector2, IReadOnlyList<MouseButton>>? OnLocalMouseUp;
    public event Action<Vector2, float>? OnLocalMouseWheel;

    public event Action<KeyModifierType, IReadOnlyList<Key>>? OnLocalKeyPress;
    public event Action<KeyModifierType, IReadOnlyList<Key>>? OnLocalKeyRelease;
    public event Action<KeyModifierType, IReadOnlyList<Key>>? OnLocalKeyRepeat;

    public ScreenInputFeature(Screen screen)
    {
        _screen = screen;
    }

    // IMouseInputFeature implementation - transform and dispatch
    public void OnMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons)
    {
        if (!IsMouseEnabled) return;
        var localPos = _screen.WorldToLocal(new Vector2(x, y));
        OnLocalMouseDown?.Invoke(localPos, buttons);
    }

    public void OnMouseMove(int x, int y)
    {
        if (!IsMouseEnabled) return;
        var localPos = _screen.WorldToLocal(new Vector2(x, y));
        OnLocalMouseMove?.Invoke(localPos);
    }

    public void OnMouseUp(int x, int y, IReadOnlyList<MouseButton> buttons)
    {
        if (!IsMouseEnabled) return;
        var localPos = _screen.WorldToLocal(new Vector2(x, y));
        OnLocalMouseUp?.Invoke(localPos, buttons);
    }

    public void OnMouseWheel(int x, int y, float delta)
    {
        if (!IsMouseEnabled) return;
        var localPos = _screen.WorldToLocal(new Vector2(x, y));
        OnLocalMouseWheel?.Invoke(localPos, delta);
    }

    // IKeyboardInputFeature implementation
    public void OnKeyPress(KeyModifierType modifier, IReadOnlyList<Key> key)
    {
        if (!IsKeyboardEnabled) return;
        OnLocalKeyPress?.Invoke(modifier, key);
    }

    public void OnKeyRelease(KeyModifierType modifier, IReadOnlyList<Key> key)
    {
        if (!IsKeyboardEnabled) return;
        OnLocalKeyRelease?.Invoke(modifier, key);
    }

    public void OnKeyRepeat(KeyModifierType modifier, IReadOnlyList<Key> key)
    {
        if (!IsKeyboardEnabled) return;
        OnLocalKeyRepeat?.Invoke(modifier, key);
    }
}
