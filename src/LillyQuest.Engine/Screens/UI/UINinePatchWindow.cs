using System;
using System.Numerics;
using LillyQuest.Core.Data.Assets;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Screens.UI;
using LillyQuest.Core.Primitives;
using Silk.NET.Maths;

namespace LillyQuest.Engine.Screens.UI;

public sealed class UINinePatchWindow : UIScreenControl
{
    private readonly List<UIScreenControl> _children = [];
    private readonly INineSliceAssetManager _nineSliceManager;
    private readonly ITextureManager _textureManager;
    private bool _isDragging;
    private Vector2 _dragOffset;

    public IReadOnlyList<UIScreenControl> Children => _children;

    public string NineSliceKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TitleFontName { get; set; } = "default_font";
    public int TitleFontSize { get; set; } = 14;
    public LyColor BorderTint { get; set; } = LyColor.White;
    public LyColor CenterTint { get; set; } = LyColor.White;
    public bool IsTitleBarEnabled { get; set; } = true;
    public bool IsWindowMovable { get; set; } = true;
    public float TitleBarHeight { get; set; } = 18f;
    public Vector4D<float> TitleMargin { get; set; } = Vector4D<float>.Zero;
    public Vector4D<float> ContentMargin { get; set; } = Vector4D<float>.Zero;
    public float NineSliceScale { get; set; } = 1f;

    public UINinePatchWindow(INineSliceAssetManager nineSliceManager, ITextureManager textureManager)
    {
        _nineSliceManager = nineSliceManager;
        _textureManager = textureManager;
    }

    public void Add(UIScreenControl control)
    {
        if (control == null)
        {
            return;
        }

        control.Parent = this;
        control.Position += new Vector2(ContentMargin.X, ContentMargin.Y);
        _children.Add(control);
    }

    public void Remove(UIScreenControl control)
    {
        if (control == null)
        {
            return;
        }

        _children.Remove(control);
        if (control.Parent == this)
        {
            control.Parent = null;
        }
    }

    public Vector2 GetTitlePosition()
    {
        var world = GetWorldPosition();
        return new Vector2(world.X + TitleMargin.X, world.Y + TitleMargin.Y);
    }

    public Vector2 GetContentOrigin()
    {
        var world = GetWorldPosition();
        return new Vector2(world.X + ContentMargin.X, world.Y + ContentMargin.Y);
    }

    public override bool HandleMouseDown(Vector2 point)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }

        foreach (var child in _children
                              .OrderByDescending(control => control.ZIndex)
                              .ThenByDescending(control => _children.IndexOf(control)))
        {
            var childBounds = child.GetBounds();

            if (point.X >= childBounds.Origin.X &&
                point.X <= childBounds.Origin.X + childBounds.Size.X &&
                point.Y >= childBounds.Origin.Y &&
                point.Y <= childBounds.Origin.Y + childBounds.Size.Y)
            {
                if (child.HandleMouseDown(point))
                {
                    return true;
                }
            }
        }

        var bounds = GetBounds();
        if (point.X < bounds.Origin.X ||
            point.X > bounds.Origin.X + bounds.Size.X ||
            point.Y < bounds.Origin.Y ||
            point.Y > bounds.Origin.Y + bounds.Size.Y)
        {
            return false;
        }

        if (IsTitleBarEnabled && IsWindowMovable && point.Y <= bounds.Origin.Y + TitleBarHeight)
        {
            _isDragging = true;
            _dragOffset = point - GetWorldPosition();
            return true;
        }

        return true;
    }

    public override bool HandleMouseMove(Vector2 point)
    {
        if (!_isDragging)
        {
            return false;
        }

        Position = point - _dragOffset;
        ClampToParent();

        return true;
    }

    public override bool HandleMouseUp(Vector2 point)
    {
        if (!_isDragging)
        {
            return false;
        }

        _isDragging = false;
        return true;
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

        DrawNineSlice(spriteBatch, texture, slice);

        if (!string.IsNullOrWhiteSpace(Title))
        {
            spriteBatch.DrawFont(TitleFontName, TitleFontSize, Title, GetTitlePosition(), LyColor.White);
        }

        foreach (var child in _children.OrderBy(control => control.ZIndex))
        {
            if (!child.IsVisible)
            {
                continue;
            }

            child.Render(spriteBatch, renderContext);
        }
    }

    private void DrawNineSlice(SpriteBatch spriteBatch, Texture2D texture, NineSliceDefinition slice)
    {
        var world = GetWorldPosition();

        var leftWidth = slice.Left.Size.X * NineSliceScale;
        var rightWidth = slice.Right.Size.X * NineSliceScale;
        var topHeight = slice.Top.Size.Y * NineSliceScale;
        var bottomHeight = slice.Bottom.Size.Y * NineSliceScale;

        var centerWidth = MathF.Max(0f, Size.X - leftWidth - rightWidth);
        var centerHeight = MathF.Max(0f, Size.Y - topHeight - bottomHeight);

        DrawSlice(spriteBatch, texture, world, new Vector2(leftWidth, topHeight), slice.TopLeft, BorderTint);
        DrawSlice(
            spriteBatch,
            texture,
            new Vector2(world.X + leftWidth + centerWidth, world.Y),
            new Vector2(rightWidth, topHeight),
            slice.TopRight,
            BorderTint
        );
        DrawSlice(
            spriteBatch,
            texture,
            new Vector2(world.X, world.Y + topHeight + centerHeight),
            new Vector2(leftWidth, bottomHeight),
            slice.BottomLeft,
            BorderTint
        );
        DrawSlice(
            spriteBatch,
            texture,
            new Vector2(world.X + leftWidth + centerWidth, world.Y + topHeight + centerHeight),
            new Vector2(rightWidth, bottomHeight),
            slice.BottomRight,
            BorderTint
        );

        DrawTiled(spriteBatch, texture, new Vector2(world.X + leftWidth, world.Y), new Vector2(centerWidth, topHeight), slice.Top, BorderTint);
        DrawTiled(
            spriteBatch,
            texture,
            new Vector2(world.X + leftWidth, world.Y + topHeight + centerHeight),
            new Vector2(centerWidth, bottomHeight),
            slice.Bottom,
            BorderTint
        );
        DrawTiled(spriteBatch, texture, new Vector2(world.X, world.Y + topHeight), new Vector2(leftWidth, centerHeight), slice.Left, BorderTint);
        DrawTiled(
            spriteBatch,
            texture,
            new Vector2(world.X + leftWidth + centerWidth, world.Y + topHeight),
            new Vector2(rightWidth, centerHeight),
            slice.Right,
            BorderTint
        );

        DrawTiled(
            spriteBatch,
            texture,
            new Vector2(world.X + leftWidth, world.Y + topHeight),
            new Vector2(centerWidth, centerHeight),
            slice.Center,
            CenterTint
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
                    new Vector2(position.X + x, position.Y + y),
                    new Vector2(drawWidth, drawHeight),
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
        var width = (sourceRect.Size.X * uScale) / texture.Width;
        var height = (sourceRect.Size.Y * vScale) / texture.Height;
        return new Rectangle<float>(u, v, width, height);
    }

    private void ClampToParent()
    {
        if (Parent == null)
        {
            return;
        }

        var maxX = Parent.Size.X - Size.X;
        var maxY = Parent.Size.Y - Size.Y;
        var clampedX = Math.Clamp(Position.X, 0f, maxX);
        var clampedY = Math.Clamp(Position.Y, 0f, maxY);
        Position = new(clampedX, clampedY);
    }
}
