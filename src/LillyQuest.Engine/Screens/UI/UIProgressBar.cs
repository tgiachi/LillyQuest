using System.Numerics;
using LillyQuest.Core.Data.Assets;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using Silk.NET.Maths;

namespace LillyQuest.Engine.Screens.UI;

public enum ProgressOrientation
{
    Horizontal,
    Vertical
}

public sealed class UIProgressBar : UIScreenControl
{
    private readonly INineSliceAssetManager _nineSliceManager;
    private readonly ITextureManager _textureManager;

    public float Min { get; set; }
    public float Max { get; set; } = 1f;
    public float Value { get; set; }
    public ProgressOrientation Orientation { get; set; } = ProgressOrientation.Horizontal;

    public string NineSliceKey { get; set; } = string.Empty;
    public float NineSliceScale { get; set; } = 1f;
    public LyColor BackgroundTint { get; set; } = LyColor.White;
    public LyColor ProgressTint { get; set; } = LyColor.White;

    public bool ShowText { get; set; } = true;
    public FontRef Font { get; set; } = new("default_font", 14, FontKind.TrueType);
    public LyColor TextColor { get; set; } = LyColor.White;

    public UIProgressBar(INineSliceAssetManager nineSliceManager, ITextureManager textureManager)
    {
        _nineSliceManager = nineSliceManager;
        _textureManager = textureManager;
    }

    public float NormalizedValue
    {
        get
        {
            if (Max <= Min)
            {
                return 0f;
            }

            return Math.Clamp((Value - Min) / (Max - Min), 0f, 1f);
        }
    }

    public Vector2 GetFillSize()
    {
        var t = NormalizedValue;

        return Orientation == ProgressOrientation.Vertical
            ? new Vector2(Size.X, Size.Y * t)
            : new Vector2(Size.X * t, Size.Y);
    }

    public Vector2 GetFillOrigin()
    {
        var world = GetWorldPosition();
        var fillSize = GetFillSize();

        return Orientation == ProgressOrientation.Vertical
            ? new Vector2(world.X, world.Y + (Size.Y - fillSize.Y))
            : world;
    }

    public string GetDisplayText()
    {
        if (!ShowText)
        {
            return string.Empty;
        }

        var percent = (int)MathF.Round(NormalizedValue * 100f);

        return $"{percent}%";
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

        DrawNineSlice(spriteBatch, texture, slice, GetWorldPosition(), Size, BackgroundTint);

        var fillSize = GetFillSize();
        if (fillSize.X > 0f && fillSize.Y > 0f)
        {
            DrawNineSlice(spriteBatch, texture, slice, GetFillOrigin(), fillSize, ProgressTint);
        }

        var text = GetDisplayText();
        if (!string.IsNullOrWhiteSpace(text))
        {
            var textSize = spriteBatch.MeasureText(Font, text);
            var world = GetWorldPosition();
            var position = world + (Size - textSize) * 0.5f;
            spriteBatch.DrawText(Font, text, position, TextColor);
        }
    }

    private void DrawNineSlice(
        SpriteBatch spriteBatch,
        Texture2D texture,
        NineSliceDefinition slice,
        Vector2 position,
        Vector2 size,
        LyColor tint
    )
    {
        var leftWidth = slice.Left.Size.X * NineSliceScale;
        var rightWidth = slice.Right.Size.X * NineSliceScale;
        var topHeight = slice.Top.Size.Y * NineSliceScale;
        var bottomHeight = slice.Bottom.Size.Y * NineSliceScale;

        var centerWidth = MathF.Max(0f, size.X - leftWidth - rightWidth);
        var centerHeight = MathF.Max(0f, size.Y - topHeight - bottomHeight);

        DrawSlice(spriteBatch, texture, position, new(leftWidth, topHeight), slice.TopLeft, tint);
        DrawSlice(
            spriteBatch,
            texture,
            new(position.X + leftWidth + centerWidth, position.Y),
            new(rightWidth, topHeight),
            slice.TopRight,
            tint
        );
        DrawSlice(
            spriteBatch,
            texture,
            new(position.X, position.Y + topHeight + centerHeight),
            new(leftWidth, bottomHeight),
            slice.BottomLeft,
            tint
        );
        DrawSlice(
            spriteBatch,
            texture,
            new(position.X + leftWidth + centerWidth, position.Y + topHeight + centerHeight),
            new(rightWidth, bottomHeight),
            slice.BottomRight,
            tint
        );

        DrawTiled(spriteBatch, texture, new(position.X + leftWidth, position.Y), new(centerWidth, topHeight), slice.Top, tint);
        DrawTiled(
            spriteBatch,
            texture,
            new(position.X + leftWidth, position.Y + topHeight + centerHeight),
            new(centerWidth, bottomHeight),
            slice.Bottom,
            tint
        );
        DrawTiled(spriteBatch, texture, new(position.X, position.Y + topHeight), new(leftWidth, centerHeight), slice.Left, tint);
        DrawTiled(
            spriteBatch,
            texture,
            new(position.X + leftWidth + centerWidth, position.Y + topHeight),
            new(rightWidth, centerHeight),
            slice.Right,
            tint
        );

        DrawTiled(
            spriteBatch,
            texture,
            new(position.X + leftWidth, position.Y + topHeight),
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

    private static Rectangle<float> ToUvRect(Texture2D texture, Rectangle<int> sourceRect, float uScale, float vScale)
    {
        var u = (float)sourceRect.Origin.X / texture.Width;
        var v = (float)sourceRect.Origin.Y / texture.Height;
        var width = sourceRect.Size.X * uScale / texture.Width;
        var height = sourceRect.Size.Y * vScale / texture.Height;

        return new(u, v, width, height);
    }
}
