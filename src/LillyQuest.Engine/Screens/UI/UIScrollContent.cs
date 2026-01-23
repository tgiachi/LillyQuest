using System.Numerics;
using LillyQuest.Core.Data.Assets;
using LillyQuest.Core.Data.Contexts;
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

        foreach (var child in _children.OrderBy(control => control.ZIndex))
        {
            if (!child.IsVisible)
            {
                continue;
            }

            child.Render(spriteBatch, renderContext);
        }
    }
}
