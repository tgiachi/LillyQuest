using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Engine.Screens.UI;

/// <summary>
/// Draggable UI window with optional title bar and child controls.
/// </summary>
public class UIWindow : UIScreenControl
{
    public string Title { get; set; } = string.Empty;
    public FontRef TitleFont { get; set; } = new("default_font", 14, FontKind.TrueType);
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
    public bool AutoSizeEnabled { get; set; }

    private bool _isDragging;
    private Vector2 _dragOffset;
    private bool _isResizing;
    private Vector2 _resizeStartAnchor;
    private Vector2 _resizeStartSize;
    private UIScreenControl? _activeChild;
    private int _autoSizeSignature;

    public UIWindow()
        => IsFocusable = true;

    public void Add(UIScreenControl control)
    {
        if (control == null)
        {
            return;
        }

        control.Position += GetContentOffset();
        AddChild(control);

        if (AutoSizeEnabled)
        {
            RecalculateAutoSize();
            _autoSizeSignature = CalculateAutoSizeSignature();
        }
    }

    public LyColor GetBackgroundColorWithAlpha()
    {
        var alpha = (byte)Math.Clamp(MathF.Round(BackgroundColor.A * BackgroundAlpha), 0f, 255f);

        return BackgroundColor.WithAlpha(alpha);
    }

    public virtual Vector2 GetContentOrigin()
        => GetWorldPosition() + GetContentOffset();

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

        var childList = Children.ToList();

        foreach (var child in childList
                              .OrderByDescending(control => control.ZIndex)
                              .ThenByDescending(control => childList.IndexOf(control)))
        {
            var childBounds = child.GetBounds();

            if (point.X >= childBounds.Origin.X &&
                point.X <= childBounds.Origin.X + childBounds.Size.X &&
                point.Y >= childBounds.Origin.Y &&
                point.Y <= childBounds.Origin.Y + childBounds.Size.Y)
            {
                if (child.HandleMouseDown(point))
                {
                    _activeChild = child;

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
        if (_activeChild != null)
        {
            return _activeChild.HandleMouseMove(point);
        }

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
        if (_activeChild != null)
        {
            var handled = _activeChild.HandleMouseUp(point);
            _activeChild = null;

            return handled;
        }

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

        RemoveChild(control);

        if (AutoSizeEnabled)
        {
            RecalculateAutoSize();
            _autoSizeSignature = CalculateAutoSizeSignature();
        }
    }

    public override void Render(SpriteBatch? spriteBatch, EngineRenderContext? renderContext)
    {
        if (spriteBatch == null || renderContext == null)
        {
            return;
        }

        spriteBatch.SetScissor((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

        RenderBackground(spriteBatch, renderContext);
        RenderTitle(spriteBatch);

        foreach (var child in Children.OrderBy(control => control.ZIndex))
        {
            if (!child.IsVisible)
            {
                continue;
            }

            child.Render(spriteBatch, renderContext);
        }

        spriteBatch.DisableScissor();
    }

    public override void Update(GameTime gameTime)
    {
        if (AutoSizeEnabled)
        {
            var signature = CalculateAutoSizeSignature();
            if (signature != _autoSizeSignature)
            {
                RecalculateAutoSize();
                _autoSizeSignature = signature;
            }
        }

        // Create snapshot to avoid collection modification during iteration
        var snapshot = Children.ToList();
        foreach (var child in snapshot)
        {
            child.Update(gameTime);
        }
    }

    protected virtual Vector2 GetContentOffset()
        => Vector2.Zero;

    protected virtual Vector4 GetContentPadding()
        => Vector4.Zero;

    public void RecalculateAutoSize()
    {
        var padding = GetContentPadding();
        var contentOffset = new Vector2(padding.X, padding.Y);
        var hasChildren = false;
        var minX = 0f;
        var minY = 0f;
        var maxX = 0f;
        var maxY = 0f;

        foreach (var child in Children)
        {
            var localPos = child.Position - contentOffset;
            var childMinX = localPos.X;
            var childMinY = localPos.Y;
            var childMaxX = localPos.X + child.Size.X;
            var childMaxY = localPos.Y + child.Size.Y;

            if (!hasChildren)
            {
                minX = childMinX;
                minY = childMinY;
                maxX = childMaxX;
                maxY = childMaxY;
                hasChildren = true;
                continue;
            }

            minX = MathF.Min(minX, childMinX);
            minY = MathF.Min(minY, childMinY);
            maxX = MathF.Max(maxX, childMaxX);
            maxY = MathF.Max(maxY, childMaxY);
        }

        var contentWidth = 0f;
        var contentHeight = 0f;

        if (hasChildren)
        {
            var minXAdjusted = MathF.Min(0f, minX);
            var minYAdjusted = MathF.Min(0f, minY);
            contentWidth = maxX - minXAdjusted;
            contentHeight = maxY - minYAdjusted;
        }

        var topPadding = IsTitleBarEnabled ? MathF.Max(TitleBarHeight, padding.Y) : padding.Y;
        var targetSize = new Vector2(
            padding.X + contentWidth + padding.Z,
            topPadding + contentHeight + padding.W
        );

        Size = new Vector2(
            Math.Clamp(targetSize.X, MinSize.X, MaxSize.X),
            Math.Clamp(targetSize.Y, MinSize.Y, MaxSize.Y)
        );
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
            spriteBatch.DrawText(TitleFont, Title, GetTitlePosition(), LyColor.White);
        }
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

    private int CalculateAutoSizeSignature()
    {
        var hash = new HashCode();
        hash.Add(Children.Count);

        foreach (var child in Children)
        {
            hash.Add(child.Position.X);
            hash.Add(child.Position.Y);
            hash.Add(child.Size.X);
            hash.Add(child.Size.Y);
        }

        return hash.ToHashCode();
    }
}
