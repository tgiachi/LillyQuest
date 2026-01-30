using System;
using System.Linq;
using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Input;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Screens;
using LillyQuest.Engine.Managers.Screens.Base;

namespace LillyQuest.Engine.Screens.Layout;

public sealed class PercentageLayoutScreen : BaseScreen
{
    private readonly List<PercentageLayoutSlot> _slots = [];
    private IScreenManager? _screenManager;
    private bool _isInitialized;
    private bool _isLoaded;
    private Vector2 _lastRootSize = Vector2.Zero;

    public void Add(
        IScreen screen,
        float xPercent,
        float yPercent,
        float widthPercent,
        float heightPercent,
        Vector2? minSize = null,
        Vector2? maxSize = null
    )
    {
        if (screen == null)
        {
            return;
        }

        if (_slots.Any(slot => ReferenceEquals(slot.Screen, screen)))
        {
            return;
        }

        var slot = new PercentageLayoutSlot(
            screen,
            new Vector4(Clamp01(xPercent), Clamp01(yPercent), Clamp01(widthPercent), Clamp01(heightPercent)),
            minSize,
            maxSize
        );
        _slots.Add(slot);

        if (_isInitialized && _screenManager != null)
        {
            screen.OnInitialize(_screenManager);
        }

        if (_isLoaded)
        {
            screen.OnLoad();
        }
    }

    public void Remove(IScreen screen)
    {
        var index = _slots.FindIndex(slot => ReferenceEquals(slot.Screen, screen));
        if (index < 0)
        {
            return;
        }

        if (_isLoaded)
        {
            _slots[index].Screen.OnUnload();
        }

        _slots.RemoveAt(index);
    }

    public void SetLayout(
        IScreen screen,
        float xPercent,
        float yPercent,
        float widthPercent,
        float heightPercent,
        Vector2? minSize = null,
        Vector2? maxSize = null
    )
    {
        var index = _slots.FindIndex(slot => ReferenceEquals(slot.Screen, screen));
        if (index < 0)
        {
            return;
        }

        _slots[index] = new PercentageLayoutSlot(
            screen,
            new Vector4(Clamp01(xPercent), Clamp01(yPercent), Clamp01(widthPercent), Clamp01(heightPercent)),
            minSize,
            maxSize
        );
    }

    public void ApplyLayout(Vector2 rootSize)
    {
        if (rootSize.X <= 0f || rootSize.Y <= 0f)
        {
            return;
        }

        Position = Vector2.Zero;
        Size = rootSize;
        _lastRootSize = rootSize;

        foreach (var slot in _slots)
        {
            var rect = slot.PercentRect;
            var position = new Vector2(rootSize.X * rect.X, rootSize.Y * rect.Y);
            var size = new Vector2(rootSize.X * rect.Z, rootSize.Y * rect.W);

            if (slot.MinSize.HasValue)
            {
                var min = slot.MinSize.Value;
                size = new Vector2(MathF.Max(size.X, min.X), MathF.Max(size.Y, min.Y));
            }

            if (slot.MaxSize.HasValue)
            {
                var max = slot.MaxSize.Value;
                size = new Vector2(MathF.Min(size.X, max.X), MathF.Min(size.Y, max.Y));
            }

            if (slot.Screen is ILayoutAwareScreen layoutAware)
            {
                layoutAware.ApplyLayout(position, size, rootSize);
            }
            else if (slot.Screen is BaseScreen baseScreen)
            {
                baseScreen.Position = position;
                baseScreen.Size = size;
            }
        }
    }

    public override IReadOnlyList<IInputConsumer>? GetChildren()
        => _slots.Count == 0
               ? null
               : _slots.Select(slot => (IInputConsumer)slot.Screen).ToList();

    public override void OnInitialize(IScreenManager screenManager)
    {
        _screenManager = screenManager;
        _isInitialized = true;

        foreach (var slot in _slots)
        {
            slot.Screen.OnInitialize(screenManager);
        }
    }

    public override void OnLoad()
    {
        _isLoaded = true;

        foreach (var slot in _slots)
        {
            slot.Screen.OnLoad();
        }
    }

    public override void OnUnload()
    {
        _isLoaded = false;

        foreach (var slot in _slots)
        {
            slot.Screen.OnUnload();
        }
    }

    public override void Render(SpriteBatch spriteBatch, EngineRenderContext renderContext)
    {
        var rootSize = renderContext.LogicalWindowSize;
        if (rootSize.X <= 0f || rootSize.Y <= 0f)
        {
            rootSize = renderContext.PhysicalWindowSize;
        }

        ApplyLayout(rootSize);

        foreach (var slot in _slots)
        {
            slot.Screen.Render(spriteBatch, renderContext);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (_lastRootSize.X > 0f && _lastRootSize.Y > 0f)
        {
            ApplyLayout(_lastRootSize);
        }

        foreach (var slot in _slots)
        {
            slot.Screen.Update(gameTime);
        }
    }

    private static float Clamp01(float value)
        => Math.Clamp(value, 0f, 1f);
}
