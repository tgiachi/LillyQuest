using System.Numerics;
using LillyQuest.Core.Data.Assets;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Data.Input;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace LillyQuest.Engine.Screens.UI;

public sealed class UITextBox : UIScreenControl
{
    private const float DefaultHorizontalPadding = 6f;

    private readonly INineSliceAssetManager _nineSliceManager;
    private readonly ITextureManager _textureManager;

    public string Text { get; set; } = string.Empty;
    public FontRef Font { get; set; } = new("default_font", 14, FontKind.TrueType);
    public string NineSliceKey { get; set; } = string.Empty;
    public float NineSliceScale { get; set; } = 2f;
    public LyColor TextColor { get; set; } = LyColor.White;
    public LyColor BackgroundTint { get; set; } = LyColor.White;
    public LyColor CenterTint { get; set; } = LyColor.White;
    public LyColor CursorColor { get; set; } = LyColor.White;
    public bool ShowCursor { get; set; } = true;
    public bool AutoHeightEnabled { get; set; } = true;
    public float VerticalPadding { get; set; } = 1f;
    public int CursorIndex { get; set; }

    public static float ComputeNineSliceAxisScale(float desiredScale, float availableSize, float startSize, float endSize)
    {
        var minSize = (startSize + endSize) * desiredScale;

        if (availableSize <= 0f || minSize <= 0f)
        {
            return desiredScale;
        }

        return availableSize < minSize ? (availableSize / (startSize + endSize)) : desiredScale;
    }

    public UITextBox(INineSliceAssetManager nineSliceManager, ITextureManager textureManager)
    {
        _nineSliceManager = nineSliceManager;
        _textureManager = textureManager;
        IsFocusable = true;
    }

    public void ApplyAutoHeight(Vector2 measured)
    {
        if (!AutoHeightEnabled)
        {
            return;
        }

        Size = new(Size.X, measured.Y + (VerticalPadding * 2f));
    }

    public void HandleTextInput(char c)
    {
        Text = Text.Insert(CursorIndex, c.ToString());
        CursorIndex = Math.Clamp(CursorIndex + 1, 0, Text.Length);
    }

    public void HandleBackspace()
    {
        if (CursorIndex <= 0 || Text.Length == 0)
        {
            return;
        }

        Text = Text.Remove(CursorIndex - 1, 1);
        CursorIndex = Math.Clamp(CursorIndex - 1, 0, Text.Length);
    }

    public override bool HandleMouseDown(Vector2 point)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }

        var bounds = GetBounds();
        var inside = point.X >= bounds.Origin.X &&
                     point.X <= bounds.Origin.X + bounds.Size.X &&
                     point.Y >= bounds.Origin.Y &&
                     point.Y <= bounds.Origin.Y + bounds.Size.Y;

        return inside;
    }

    public override bool HandleKeyPress(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        return HandleKeys(modifier, keys);
    }

    public override bool HandleKeyRepeat(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        return HandleKeys(modifier, keys);
    }

    public override void Render(SpriteBatch? spriteBatch, EngineRenderContext? renderContext)
    {
        if (spriteBatch == null || renderContext == null)
        {
            return;
        }

        var measureText = string.IsNullOrEmpty(Text) ? " " : Text;
        var textSize = spriteBatch.MeasureText(Font, measureText);
        ApplyAutoHeight(textSize);

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

        DrawNineSlice(spriteBatch, texture, slice, BackgroundTint, CenterTint);

        if (!string.IsNullOrWhiteSpace(Text))
        {
            var world = GetWorldPosition();
            var position = new Vector2(
                world.X + DefaultHorizontalPadding,
                world.Y + (Size.Y - textSize.Y) * 0.5f
            );
            spriteBatch.DrawText(Font, Text, position, TextColor);
        }

        if (ShowCursor)
        {
            var world = GetWorldPosition();
            var safeIndex = Math.Clamp(CursorIndex, 0, Text.Length);
            var beforeText = safeIndex > 0 ? Text[..safeIndex] : string.Empty;
            var beforeSize = string.IsNullOrEmpty(beforeText)
                                 ? Vector2.Zero
                                 : spriteBatch.MeasureText(Font, beforeText);
            var caretPosition = new Vector2(
                world.X + DefaultHorizontalPadding + beforeSize.X,
                world.Y + (Size.Y - textSize.Y) * 0.5f
            );
            var caretSize = new Vector2(1f, textSize.Y);
            var uv = new Rectangle<float>(0f, 0f, 1f, 1f);
            spriteBatch.Draw(
                _textureManager.DefaultWhiteTexture,
                caretPosition,
                caretSize,
                CursorColor,
                0f,
                Vector2.Zero,
                uv,
                0f
            );
        }
    }

    private bool HandleKeys(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        var handled = false;
        var isShift = modifier.HasFlag(KeyModifierType.Shift);

        foreach (var key in keys)
        {
            switch (key)
            {
                case Key.Left:
                    CursorIndex = Math.Max(0, CursorIndex - 1);
                    handled = true;

                    break;
                case Key.Right:
                    CursorIndex = Math.Min(Text.Length, CursorIndex + 1);
                    handled = true;

                    break;
                case Key.Backspace:
                    HandleBackspace();
                    handled = true;

                    break;
                default:
                    if (TryGetCharacter(key, isShift, out var character))
                    {
                        HandleTextInput(character);
                        handled = true;
                    }

                    break;
            }
        }

        return handled;
    }

    private static bool TryGetCharacter(Key key, bool isShift, out char character)
    {
        character = default;

        if (key is >= Key.A and <= Key.Z)
        {
            character = (char)('a' + (key - Key.A));
            character = isShift ? char.ToUpperInvariant(character) : character;

            return true;
        }

        if (key is >= Key.Number0 and <= Key.Number9)
        {
            character = (char)('0' + (key - Key.Number0));

            return true;
        }

        if (key == Key.Space)
        {
            character = ' ';

            return true;
        }

        return false;
    }

    private void DrawNineSlice(
        SpriteBatch spriteBatch,
        Texture2D texture,
        NineSliceDefinition slice,
        LyColor borderTint,
        LyColor centerTint
    )
    {
        var world = GetWorldPosition();

        var scaleX = ComputeNineSliceAxisScale(NineSliceScale, Size.X, slice.Left.Size.X, slice.Right.Size.X);
        var scaleY = ComputeNineSliceAxisScale(NineSliceScale, Size.Y, slice.Top.Size.Y, slice.Bottom.Size.Y);

        var leftWidth = slice.Left.Size.X * scaleX;
        var rightWidth = slice.Right.Size.X * scaleX;
        var topHeight = slice.Top.Size.Y * scaleY;
        var bottomHeight = slice.Bottom.Size.Y * scaleY;

        var centerWidth = MathF.Max(0f, Size.X - leftWidth - rightWidth);
        var centerHeight = MathF.Max(0f, Size.Y - topHeight - bottomHeight);

        DrawSlice(spriteBatch, texture, world, new(leftWidth, topHeight), slice.TopLeft, borderTint);
        DrawSlice(
            spriteBatch,
            texture,
            new(world.X + leftWidth + centerWidth, world.Y),
            new(rightWidth, topHeight),
            slice.TopRight,
            borderTint
        );
        DrawSlice(
            spriteBatch,
            texture,
            new(world.X, world.Y + topHeight + centerHeight),
            new(leftWidth, bottomHeight),
            slice.BottomLeft,
            borderTint
        );
        DrawSlice(
            spriteBatch,
            texture,
            new(world.X + leftWidth + centerWidth, world.Y + topHeight + centerHeight),
            new(rightWidth, bottomHeight),
            slice.BottomRight,
            borderTint
        );

        DrawTiled(
            spriteBatch,
            texture,
            new(world.X + leftWidth, world.Y),
            new(centerWidth, topHeight),
            slice.Top,
            borderTint,
            scaleX,
            scaleY
        );
        DrawTiled(
            spriteBatch,
            texture,
            new(world.X + leftWidth, world.Y + topHeight + centerHeight),
            new(centerWidth, bottomHeight),
            slice.Bottom,
            borderTint,
            scaleX,
            scaleY
        );
        DrawTiled(
            spriteBatch,
            texture,
            new(world.X, world.Y + topHeight),
            new(leftWidth, centerHeight),
            slice.Left,
            borderTint,
            scaleX,
            scaleY
        );
        DrawTiled(
            spriteBatch,
            texture,
            new(world.X + leftWidth + centerWidth, world.Y + topHeight),
            new(rightWidth, centerHeight),
            slice.Right,
            borderTint,
            scaleX,
            scaleY
        );

        DrawTiled(
            spriteBatch,
            texture,
            new(world.X + leftWidth, world.Y + topHeight),
            new(centerWidth, centerHeight),
            slice.Center,
            centerTint,
            scaleX,
            scaleY
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
        LyColor tint,
        float scaleX,
        float scaleY
    )
    {
        if (size.X <= 0f || size.Y <= 0f)
        {
            return;
        }

        var tileWidth = sourceRect.Size.X * scaleX;
        var tileHeight = sourceRect.Size.Y * scaleY;

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

    private static Rectangle<float> ToUvRect(Texture2D texture, Rectangle<int> sourceRect, float uScale, float vScale)
    {
        var u = (float)sourceRect.Origin.X / texture.Width;
        var v = (float)sourceRect.Origin.Y / texture.Height;
        var width = sourceRect.Size.X * uScale / texture.Width;
        var height = sourceRect.Size.Y * vScale / texture.Height;

        return new(u, v, width, height);
    }
}
