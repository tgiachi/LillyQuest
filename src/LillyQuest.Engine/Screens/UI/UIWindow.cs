using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Engine.Screens.UI;

/// <summary>
/// Draggable UI window with optional title bar and child controls.
/// </summary>
public sealed class UIWindow : UIScreenControl
{
    private readonly List<UIScreenControl> _children = [];

    public IReadOnlyList<UIScreenControl> Children => _children;

    public string Title { get; set; } = string.Empty;
    public string TitleFontName { get; set; } = "default_font";
    public int TitleFontSize { get; set; } = 14;
    public bool IsTitleBarEnabled { get; set; } = true;
    public bool IsWindowMovable { get; set; } = true;
    public LyColor BackgroundColor { get; set; } = LyColor.Black;
    public float BackgroundAlpha { get; set; } = 1f;
    public LyColor BorderColor { get; set; } = LyColor.White;
    public float BorderThickness { get; set; } = 1f;
    public float TitleBarHeight { get; set; } = 18f;

    private bool _isDragging;
    private Vector2 _dragOffset;

    public void Add(UIScreenControl control)
    {
        if (control == null)
        {
            return;
        }

        control.Parent = this;
        _children.Add(control);
    }

    public LyColor GetBackgroundColorWithAlpha()
    {
        var alpha = (byte)Math.Clamp(MathF.Round(BackgroundColor.A * BackgroundAlpha), 0f, 255f);

        return BackgroundColor.WithAlpha(alpha);
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

    public override void Render(SpriteBatch? spriteBatch, EngineRenderContext? renderContext)
    {
        if (spriteBatch == null || renderContext == null)
        {
            return;
        }

        var world = GetWorldPosition();
        var background = GetBackgroundColorWithAlpha();
        spriteBatch.DrawRectangle(world, Size, background);
        spriteBatch.DrawRectangleOutline(world, Size, BorderColor, BorderThickness);

        if (IsTitleBarEnabled)
        {
            var titlebarSize = new Vector2(Size.X, TitleBarHeight);
            spriteBatch.DrawRectangle(world, titlebarSize, background);

            if (!string.IsNullOrWhiteSpace(Title))
            {
                spriteBatch.DrawFont(TitleFontName, TitleFontSize, Title, world + new Vector2(4, 2), LyColor.White);
            }
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
