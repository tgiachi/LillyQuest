using System.Numerics;
using LillyQuest.Core.Data.Assets;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using Silk.NET.Maths;

namespace LillyQuest.Engine.Screens.UI;

public enum UIButtonState
{
    Idle,
    Hovered,
    Pressed
}

public sealed class UIButton : UIScreenControl
{
    private readonly INineSliceAssetManager _nineSliceManager;
    private readonly ITextureManager _textureManager;
    private readonly IFontManager _fontManager;

    private LyColor _targetTint = LyColor.White;
    private float _transitionElapsed;
    private bool _isHovered;
    private LyColor _transitionStartTint = LyColor.White;

    public UIButtonState State { get; private set; } = UIButtonState.Idle;

    public LyColor CurrentTint { get; private set; } = LyColor.White;

    public string NineSliceKey { get; set; } = string.Empty;
    public float NineSliceScale { get; set; } = 1f;
    public string Text { get; set; } = string.Empty;
    public string FontName { get; set; } = "default_font";
    public int FontSize { get; set; } = 14;
    public LyColor TextColor { get; set; } = LyColor.White;

    public LyColor IdleTint { get; set; } = LyColor.White;
    public LyColor HoveredTint { get; set; } = LyColor.White;
    public LyColor PressedTint { get; set; } = LyColor.White;
    public float TransitionTime { get; set; } = 0.15f;

    private bool _tintInitialized;

    public Action? OnClick { get; set; }
    public Action? OnHover { get; set; }

    public UIButton(INineSliceAssetManager nineSliceManager, ITextureManager textureManager, IFontManager fontManager)
    {
        _nineSliceManager = nineSliceManager;
        _textureManager = textureManager;
        _fontManager = fontManager;
        CurrentTint = IdleTint;
        _targetTint = IdleTint;
        _transitionStartTint = IdleTint;
        _tintInitialized = true;
        IsFocusable = true;
    }

    public void Add(UIScreenControl control)
    {
        if (control == null)
        {
            return;
        }

        AddChild(control);
    }

    public override bool HandleMouseDown(Vector2 point)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }

        SyncIdleTintIfNeeded();

        var bounds = GetBounds();
        var inside = point.X >= bounds.Origin.X &&
                     point.X <= bounds.Origin.X + bounds.Size.X &&
                     point.Y >= bounds.Origin.Y &&
                     point.Y <= bounds.Origin.Y + bounds.Size.Y;

        if (!inside)
        {
            return false;
        }

        SetState(UIButtonState.Pressed);

        return true;
    }

    public override bool HandleMouseMove(Vector2 point)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }

        SyncIdleTintIfNeeded();

        var bounds = GetBounds();
        var inside = point.X >= bounds.Origin.X &&
                     point.X <= bounds.Origin.X + bounds.Size.X &&
                     point.Y >= bounds.Origin.Y &&
                     point.Y <= bounds.Origin.Y + bounds.Size.Y;

        if (inside && !_isHovered)
        {
            _isHovered = true;
            OnHover?.Invoke();
            SetState(UIButtonState.Hovered);
        }
        else if (!inside && _isHovered)
        {
            _isHovered = false;

            if (State != UIButtonState.Pressed)
            {
                SetState(UIButtonState.Idle);
            }
        }

        return inside;
    }

    public override bool HandleMouseUp(Vector2 point)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }

        SyncIdleTintIfNeeded();

        var bounds = GetBounds();
        var inside = point.X >= bounds.Origin.X &&
                     point.X <= bounds.Origin.X + bounds.Size.X &&
                     point.Y >= bounds.Origin.Y &&
                     point.Y <= bounds.Origin.Y + bounds.Size.Y;

        if (State == UIButtonState.Pressed && inside)
        {
            OnClick?.Invoke();
            SetState(UIButtonState.Hovered);

            return true;
        }

        if (!inside)
        {
            SetState(UIButtonState.Idle);
        }

        return inside;
    }

    public void Remove(UIScreenControl control)
    {
        if (control == null)
        {
            return;
        }

        RemoveChild(control);
    }

    public override void Render(SpriteBatch? spriteBatch, EngineRenderContext? renderContext)
    {
        if (spriteBatch == null || renderContext == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(NineSliceKey))
        {
            return;
        }

        if (!_nineSliceManager.TryGetNineSlice(NineSliceKey, out var slice))
        {
            return;
        }

        if (!_textureManager.TryGetTexture(slice.TextureName, out var texture))
        {
            return;
        }

        DrawNineSlice(spriteBatch, texture, slice, CurrentTint);

        if (!string.IsNullOrWhiteSpace(Text))
        {
            var textSize = _fontManager.MeasureText(FontName, FontSize, Text);
            var world = GetWorldPosition();
            var position = world + (Size - textSize) * 0.5f;
            spriteBatch.DrawFont(FontName, FontSize, Text, position, TextColor);
        }
    }

    public override void Update(GameTime gameTime)
    {
        SyncIdleTintIfNeeded();

        if (TransitionTime <= 0f)
        {
            CurrentTint = _targetTint;

            return;
        }

        _transitionElapsed += (float)gameTime.Elapsed.TotalSeconds;
        var t = Math.Clamp(_transitionElapsed / TransitionTime, 0f, 1f);
        CurrentTint = LerpColor(_transitionStartTint, _targetTint, t);
    }

    private void DrawNineSlice(SpriteBatch spriteBatch, Texture2D texture, NineSliceDefinition slice, LyColor tint)
    {
        var world = GetWorldPosition();

        var leftWidth = slice.Left.Size.X * NineSliceScale;
        var rightWidth = slice.Right.Size.X * NineSliceScale;
        var topHeight = slice.Top.Size.Y * NineSliceScale;
        var bottomHeight = slice.Bottom.Size.Y * NineSliceScale;

        var centerWidth = MathF.Max(0f, Size.X - leftWidth - rightWidth);
        var centerHeight = MathF.Max(0f, Size.Y - topHeight - bottomHeight);

        DrawSlice(spriteBatch, texture, world, new(leftWidth, topHeight), slice.TopLeft, tint);
        DrawSlice(
            spriteBatch,
            texture,
            new(world.X + leftWidth + centerWidth, world.Y),
            new(rightWidth, topHeight),
            slice.TopRight,
            tint
        );
        DrawSlice(
            spriteBatch,
            texture,
            new(world.X, world.Y + topHeight + centerHeight),
            new(leftWidth, bottomHeight),
            slice.BottomLeft,
            tint
        );
        DrawSlice(
            spriteBatch,
            texture,
            new(world.X + leftWidth + centerWidth, world.Y + topHeight + centerHeight),
            new(rightWidth, bottomHeight),
            slice.BottomRight,
            tint
        );

        DrawTiled(spriteBatch, texture, new(world.X + leftWidth, world.Y), new(centerWidth, topHeight), slice.Top, tint);
        DrawTiled(
            spriteBatch,
            texture,
            new(world.X + leftWidth, world.Y + topHeight + centerHeight),
            new(centerWidth, bottomHeight),
            slice.Bottom,
            tint
        );
        DrawTiled(spriteBatch, texture, new(world.X, world.Y + topHeight), new(leftWidth, centerHeight), slice.Left, tint);
        DrawTiled(
            spriteBatch,
            texture,
            new(world.X + leftWidth + centerWidth, world.Y + topHeight),
            new(rightWidth, centerHeight),
            slice.Right,
            tint
        );

        DrawTiled(
            spriteBatch,
            texture,
            new(world.X + leftWidth, world.Y + topHeight),
            new(centerWidth, centerHeight),
            slice.Center,
            tint
        );
    }

    private void DrawSlice(
        SpriteBatch spriteBatch,
        Texture2D texture,
        Vector2 position,
        Vector2 size,
        Rectangle<int> sourceRect,
        LyColor tint
    )
    {
        if (size.X <= 0f || size.Y <= 0f)
        {
            return;
        }

        var uv = ToUvRect(texture, sourceRect, 1f, 1f);
        spriteBatch.Draw(texture, position, size, tint, 0f, Vector2.Zero, uv, 0f);
    }

    private void DrawTiled(
        SpriteBatch spriteBatch,
        Texture2D texture,
        Vector2 position,
        Vector2 size,
        Rectangle<int> sourceRect,
        LyColor tint
    )
    {
        if (size.X <= 0f || size.Y <= 0f)
        {
            return;
        }

        var tileWidth = sourceRect.Size.X * NineSliceScale;
        var tileHeight = sourceRect.Size.Y * NineSliceScale;

        if (tileWidth <= 0f || tileHeight <= 0f)
        {
            return;
        }

        for (var y = 0f; y < size.Y; y += tileHeight)
        {
            var drawHeight = MathF.Min(tileHeight, size.Y - y);
            var vScale = drawHeight / tileHeight;

            for (var x = 0f; x < size.X; x += tileWidth)
            {
                var drawWidth = MathF.Min(tileWidth, size.X - x);
                var uScale = drawWidth / tileWidth;
                var uv = ToUvRect(texture, sourceRect, uScale, vScale);
                spriteBatch.Draw(
                    texture,
                    new(position.X + x, position.Y + y),
                    new(drawWidth, drawHeight),
                    tint,
                    0f,
                    Vector2.Zero,
                    uv,
                    0f
                );
            }
        }
    }

    private static LyColor LerpColor(LyColor start, LyColor end, float t)
    {
        var r = (byte)Math.Clamp(start.R + (end.R - start.R) * t, 0, 255);
        var g = (byte)Math.Clamp(start.G + (end.G - start.G) * t, 0, 255);
        var b = (byte)Math.Clamp(start.B + (end.B - start.B) * t, 0, 255);
        var a = (byte)Math.Clamp(start.A + (end.A - start.A) * t, 0, 255);

        return new(r, g, b, a);
    }

    private void SetState(UIButtonState state)
    {
        if (State == state)
        {
            return;
        }

        State = state;
        _transitionElapsed = 0f;

        if (!_tintInitialized)
        {
            CurrentTint = IdleTint;
            _tintInitialized = true;
        }
        _transitionStartTint = CurrentTint;
        _targetTint = state switch
        {
            UIButtonState.Idle    => IdleTint,
            UIButtonState.Hovered => HoveredTint,
            UIButtonState.Pressed => PressedTint,
            _                     => IdleTint
        };

        if (TransitionTime <= 0f)
        {
            CurrentTint = _targetTint;
        }
    }

    private void SyncIdleTintIfNeeded()
    {
        if (!_tintInitialized)
        {
            CurrentTint = IdleTint;
            _targetTint = IdleTint;
            _transitionStartTint = IdleTint;
            _tintInitialized = true;

            return;
        }

        if (State == UIButtonState.Idle && _transitionElapsed == 0f)
        {
            CurrentTint = IdleTint;
            _targetTint = IdleTint;
            _transitionStartTint = IdleTint;
        }
    }

    private static Rectangle<float> ToUvRect(Texture2D texture, Rectangle<int> sourceRect, float uScale, float vScale)
    {
        var u = (float)sourceRect.Origin.X / texture.Width;
        var v = (float)sourceRect.Origin.Y / texture.Height;
        var width = sourceRect.Size.X * uScale / texture.Width;
        var height = sourceRect.Size.Y * vScale / texture.Height;

        return new(u, v, width, height);
    }
}
