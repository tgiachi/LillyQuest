using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Engine.Screens.UI;

/// <summary>
/// Draggable UI window with optional title bar and child controls.
/// </summary>
public class UIWindow : UIScreenControl
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
    public bool IsModal { get; set; }
    public bool IsResizable { get; set; } = true;
    public Vector2 MinSize { get; set; } = Vector2.Zero;
    public Vector2 MaxSize { get; set; } = new(float.PositiveInfinity, float.PositiveInfinity);
    public Vector2 ResizeHandleSize { get; set; } = new(10f, 10f);

    private bool _isDragging;
    private Vector2 _dragOffset;
    private bool _isResizing;
    private Vector2 _resizeStartAnchor;
    private Vector2 _resizeStartSize;

    public UIWindow()
    {
        IsFocusable = true;
    }

    public void Add(UIScreenControl control)
    {
        if (control == null)
        {
            return;
        }

        control.Parent = this;
        control.Position += GetContentOffset();
        _children.Add(control);
    }

    public LyColor GetBackgroundColorWithAlpha()
    {
        var alpha = (byte)Math.Clamp(MathF.Round(BackgroundColor.A * BackgroundAlpha), 0f, 255f);

        return BackgroundColor.WithAlpha(alpha);
    }

    public virtual Vector2 GetContentOrigin()
        => GetWorldPosition() + GetContentOffset();

    protected virtual Vector2 GetContentOffset()
        => Vector2.Zero;

    public virtual Vector2 GetTitlePosition()
        => GetWorldPosition() + new Vector2(4f, 2f);

    public override bool HandleMouseDown(Vector2 point)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }

        if (OnMouseDown?.Invoke(point) == true)
        {
            return true;
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

        if (IsPointInResizeHandle(point))
        {
            if (!IsResizable)
            {
                return false;
            }

            _isResizing = true;
            _resizeStartAnchor = GetWorldPosition() + Size;
            _resizeStartSize = Size;

            return true;
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
        if (_isResizing)
        {
            ApplyResize(point);

            return true;
        }

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
        if (_isResizing)
        {
            _isResizing = false;

            return true;
        }

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

        RenderBackground(spriteBatch, renderContext);
        RenderTitle(spriteBatch);

        foreach (var child in _children.OrderBy(control => control.ZIndex))
        {
            if (!child.IsVisible)
            {
                continue;
            }

            child.Render(spriteBatch, renderContext);
        }
    }

    public override void Update(GameTime gameTime)
    {
        foreach (var child in _children)
        {
            child.Update(gameTime);
        }
    }

    protected virtual void RenderBackground(SpriteBatch spriteBatch, EngineRenderContext renderContext)
    {
        var world = GetWorldPosition();
        var background = GetBackgroundColorWithAlpha();
        spriteBatch.DrawRectangle(world, Size, background);
        spriteBatch.DrawRectangleOutline(world, Size, BorderColor, BorderThickness);

        if (IsTitleBarEnabled)
        {
            var titlebarSize = new Vector2(Size.X, TitleBarHeight);
            spriteBatch.DrawRectangle(world, titlebarSize, background);
        }
    }

    protected virtual void RenderTitle(SpriteBatch spriteBatch)
    {
        if (!string.IsNullOrWhiteSpace(Title))
        {
            spriteBatch.DrawFont(TitleFontName, TitleFontSize, Title, GetTitlePosition(), LyColor.White);
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

    private bool IsPointInResizeHandle(Vector2 point)
    {
        var world = GetWorldPosition();
        var handleOrigin = new Vector2(
            world.X + Size.X - ResizeHandleSize.X,
            world.Y + Size.Y - ResizeHandleSize.Y
        );

        return point.X >= handleOrigin.X &&
               point.X <= handleOrigin.X + ResizeHandleSize.X &&
               point.Y >= handleOrigin.Y &&
               point.Y <= handleOrigin.Y + ResizeHandleSize.Y;
    }

    private void ApplyResize(Vector2 point)
    {
        var delta = point - _resizeStartAnchor;
        var target = _resizeStartSize + delta;
        var clamped = new Vector2(
            Math.Clamp(target.X, MinSize.X, MaxSize.X),
            Math.Clamp(target.Y, MinSize.Y, MaxSize.Y)
        );

        Size = clamped;
        ClampToParent();
    }
}
