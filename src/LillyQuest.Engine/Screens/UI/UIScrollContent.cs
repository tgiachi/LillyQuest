using System.Numerics;
using LillyQuest.Core.Data.Assets;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using Silk.NET.Maths;

namespace LillyQuest.Engine.Screens.UI;

public sealed class UIScrollContent : UIScreenControl
{
    private readonly List<UIScreenControl> _children = [];
    private readonly INineSliceAssetManager _nineSliceManager;
    private readonly ITextureManager _textureManager;

    public IReadOnlyList<UIScreenControl> Children => _children;

    public Vector2 ContentSize { get; set; } = Vector2.Zero;
    public Vector2 ScrollOffset { get; set; } = Vector2.Zero;

    public bool EnableVerticalScroll { get; set; } = true;
    public bool EnableHorizontalScroll { get; set; } = true;
    public float ScrollSpeed { get; set; } = 24f;

    public float ScrollbarThickness { get; set; } = 12f;
    public float MinThumbSize { get; set; } = 16f;
    public LyColor ScrollbarTint { get; set; } = LyColor.White;

    public string ScrollbarTextureName { get; set; } = string.Empty;
    public string VerticalTrackElement { get; set; } = "scroll.v.track";
    public string VerticalThumbElement { get; set; } = "scroll.v.thumb";
    public string HorizontalTrackElement { get; set; } = "scroll.h.track";
    public string HorizontalThumbElement { get; set; } = "scroll.h.thumb";

    public UIScrollContent(INineSliceAssetManager nineSliceManager, ITextureManager textureManager)
    {
        _nineSliceManager = nineSliceManager;
        _textureManager = textureManager;
        IsFocusable = true;
    }

    public void Add(UIScreenControl control)
    {
        if (control == null)
        {
            return;
        }

        control.Parent = this;
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

    public Rectangle<float> GetViewportBounds()
    {
        var world = GetWorldPosition();
        var width = Size.X - (EnableVerticalScroll ? ScrollbarThickness : 0f);
        var height = Size.Y - (EnableHorizontalScroll ? ScrollbarThickness : 0f);
        width = MathF.Max(0f, width);
        height = MathF.Max(0f, height);
        return new Rectangle<float>(world.X, world.Y, width, height);
    }

    public Rectangle<float> GetVerticalThumbRect()
    {
        if (!EnableVerticalScroll)
        {
            return new Rectangle<float>(0f, 0f, 0f, 0f);
        }

        var viewport = GetViewportBounds();
        var trackHeight = viewport.Size.Y;
        var trackX = viewport.Origin.X + viewport.Size.X;
        var trackY = viewport.Origin.Y;
        var maxOffset = MathF.Max(0f, ContentSize.Y - viewport.Size.Y);

        var thumbHeight = 0f;
        if (ContentSize.Y > 0f)
        {
            thumbHeight = viewport.Size.Y * (viewport.Size.Y / ContentSize.Y);
        }

        if (thumbHeight < MinThumbSize)
        {
            thumbHeight = MinThumbSize;
        }

        thumbHeight = MathF.Min(trackHeight, thumbHeight);

        var scrollPercent = maxOffset <= 0f ? 0f : Math.Clamp(ScrollOffset.Y / maxOffset, 0f, 1f);
        var thumbY = trackY + (trackHeight - thumbHeight) * scrollPercent;

        return new Rectangle<float>(trackX, thumbY, ScrollbarThickness, thumbHeight);
    }

    public Rectangle<float> GetHorizontalThumbRect()
    {
        if (!EnableHorizontalScroll)
        {
            return new Rectangle<float>(0f, 0f, 0f, 0f);
        }

        var viewport = GetViewportBounds();
        var trackWidth = viewport.Size.X;
        var trackX = viewport.Origin.X;
        var trackY = viewport.Origin.Y + viewport.Size.Y;
        var maxOffset = MathF.Max(0f, ContentSize.X - viewport.Size.X);

        var thumbWidth = 0f;
        if (ContentSize.X > 0f)
        {
            thumbWidth = viewport.Size.X * (viewport.Size.X / ContentSize.X);
        }

        if (thumbWidth < MinThumbSize)
        {
            thumbWidth = MinThumbSize;
        }

        thumbWidth = MathF.Min(trackWidth, thumbWidth);

        var scrollPercent = maxOffset <= 0f ? 0f : Math.Clamp(ScrollOffset.X / maxOffset, 0f, 1f);
        var thumbX = trackX + (trackWidth - thumbWidth) * scrollPercent;

        return new Rectangle<float>(thumbX, trackY, thumbWidth, ScrollbarThickness);
    }

    public Rectangle<float> GetVerticalTrackRect()
    {
        if (!EnableVerticalScroll)
        {
            return new Rectangle<float>(0f, 0f, 0f, 0f);
        }

        var viewport = GetViewportBounds();
        return new Rectangle<float>(
            viewport.Origin.X + viewport.Size.X,
            viewport.Origin.Y,
            ScrollbarThickness,
            viewport.Size.Y
        );
    }

    public Rectangle<float> GetHorizontalTrackRect()
    {
        if (!EnableHorizontalScroll)
        {
            return new Rectangle<float>(0f, 0f, 0f, 0f);
        }

        var viewport = GetViewportBounds();
        return new Rectangle<float>(
            viewport.Origin.X,
            viewport.Origin.Y + viewport.Size.Y,
            viewport.Size.X,
            ScrollbarThickness
        );
    }

    private Vector2 GetMaxScrollOffset(Rectangle<float> viewport)
    {
        var maxX = MathF.Max(0f, ContentSize.X - viewport.Size.X);
        var maxY = MathF.Max(0f, ContentSize.Y - viewport.Size.Y);
        return new Vector2(maxX, maxY);
    }

    private Vector2 ClampScroll(Rectangle<float> viewport)
    {
        var max = GetMaxScrollOffset(viewport);
        return new Vector2(
            Math.Clamp(ScrollOffset.X, 0f, max.X),
            Math.Clamp(ScrollOffset.Y, 0f, max.Y)
        );
    }

    public override void Render(SpriteBatch? spriteBatch, EngineRenderContext? renderContext)
    {
        if (spriteBatch == null || renderContext == null)
        {
            return;
        }

        var viewport = GetViewportBounds();
        ScrollOffset = ClampScroll(viewport);

        if (viewport.Size.X <= 0f || viewport.Size.Y <= 0f)
        {
            return;
        }

        spriteBatch.SetScissor(
            (int)viewport.Origin.X,
            (int)viewport.Origin.Y,
            (int)viewport.Size.X,
            (int)viewport.Size.Y
        );

        spriteBatch.PushTranslation(-ScrollOffset);

        foreach (var child in _children.OrderBy(control => control.ZIndex))
        {
            if (!child.IsVisible)
            {
                continue;
            }

            child.Render(spriteBatch, renderContext);
        }

        spriteBatch.PopTranslation();
        spriteBatch.DisableScissor();

        RenderScrollbars(spriteBatch);
    }

    public override bool HandleMouseWheel(Vector2 point, float delta)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }

        var bounds = GetBounds();
        if (point.X < bounds.Origin.X ||
            point.X > bounds.Origin.X + bounds.Size.X ||
            point.Y < bounds.Origin.Y ||
            point.Y > bounds.Origin.Y + bounds.Size.Y)
        {
            return false;
        }

        var viewport = GetViewportBounds();

        if (EnableVerticalScroll && ContentSize.Y > viewport.Size.Y)
        {
            ScrollOffset = new Vector2(ScrollOffset.X, ScrollOffset.Y + delta * ScrollSpeed);
        }
        else if (EnableHorizontalScroll && ContentSize.X > viewport.Size.X)
        {
            ScrollOffset = new Vector2(ScrollOffset.X + delta * ScrollSpeed, ScrollOffset.Y);
        }
        else
        {
            return false;
        }

        ScrollOffset = ClampScroll(viewport);
        return true;
    }

    private void RenderScrollbars(SpriteBatch spriteBatch)
    {
        if (string.IsNullOrWhiteSpace(ScrollbarTextureName))
        {
            return;
        }

        if (!_textureManager.TryGetTexture(ScrollbarTextureName, out var texture))
        {
            return;
        }

        if (EnableVerticalScroll)
        {
            if (_nineSliceManager.TryGetTexturePatch(ScrollbarTextureName, VerticalTrackElement, out var trackPatch))
            {
                DrawPatch(spriteBatch, texture, GetVerticalTrackRect(), trackPatch.Section, ScrollbarTint);
            }

            if (_nineSliceManager.TryGetTexturePatch(ScrollbarTextureName, VerticalThumbElement, out var thumbPatch))
            {
                DrawPatch(spriteBatch, texture, GetVerticalThumbRect(), thumbPatch.Section, ScrollbarTint);
            }
        }

        if (EnableHorizontalScroll)
        {
            if (_nineSliceManager.TryGetTexturePatch(ScrollbarTextureName, HorizontalTrackElement, out var trackPatch))
            {
                DrawPatch(spriteBatch, texture, GetHorizontalTrackRect(), trackPatch.Section, ScrollbarTint);
            }

            if (_nineSliceManager.TryGetTexturePatch(ScrollbarTextureName, HorizontalThumbElement, out var thumbPatch))
            {
                DrawPatch(spriteBatch, texture, GetHorizontalThumbRect(), thumbPatch.Section, ScrollbarTint);
            }
        }
    }

    private static void DrawPatch(
        SpriteBatch spriteBatch,
        Texture2D texture,
        Rectangle<float> dest,
        Rectangle<int> source,
        LyColor tint
    )
    {
        if (dest.Size.X <= 0f || dest.Size.Y <= 0f)
        {
            return;
        }

        var uv = ToUvRect(texture, source);
        spriteBatch.Draw(
            texture,
            new Vector2(dest.Origin.X, dest.Origin.Y),
            new Vector2(dest.Size.X, dest.Size.Y),
            tint,
            0f,
            Vector2.Zero,
            uv,
            0f
        );
    }

    private static Rectangle<float> ToUvRect(Texture2D texture, Rectangle<int> sourceRect)
    {
        var u = (float)sourceRect.Origin.X / texture.Width;
        var v = (float)sourceRect.Origin.Y / texture.Height;
        var width = (float)sourceRect.Size.X / texture.Width;
        var height = (float)sourceRect.Size.Y / texture.Height;
        return new Rectangle<float>(u, v, width, height);
    }
}
