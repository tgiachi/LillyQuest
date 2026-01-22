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
    public string Title { get; set; } = string.Empty;
    public bool IsTitleBarEnabled { get; set; } = true;
    public bool IsWindowMovable { get; set; } = true;
    public LyColor BackgroundColor { get; set; } = LyColor.Black;
    public float BackgroundAlpha { get; set; } = 1f;
    public LyColor BorderColor { get; set; } = LyColor.White;
    public float BorderThickness { get; set; } = 1f;
    public float TitleBarHeight { get; set; } = 18f;

    private bool _isDragging;
    private Vector2 _dragOffset;

    public override bool HandleMouseDown(Vector2 point)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }

        var bounds = GetBounds();
        if (point.X < bounds.Origin.X || point.X > bounds.Origin.X + bounds.Size.X ||
            point.Y < bounds.Origin.Y || point.Y > bounds.Origin.Y + bounds.Size.Y)
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
    }
}
