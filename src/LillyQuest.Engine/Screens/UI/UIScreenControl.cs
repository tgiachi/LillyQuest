using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using Silk.NET.Maths;

namespace LillyQuest.Engine.Screens.UI;

/// <summary>
/// Base UI control for pixel-based UI.
/// </summary>
public class UIScreenControl
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Size { get; set; } = Vector2.Zero;
    public UIAnchor Anchor { get; set; } = UIAnchor.TopLeft;
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public bool IsFocusable { get; set; }
    public int ZIndex { get; set; }

    public UIScreenControl? Parent { get; set; }

    public Func<Vector2, bool>? OnMouseDown { get; set; }
    public Func<Vector2, bool>? OnMouseMove { get; set; }
    public Func<Vector2, bool>? OnMouseUp { get; set; }

    /// <summary>
    /// Gets the bounds of the control in world space.
    /// </summary>
    public Rectangle<float> GetBounds()
    {
        var world = GetWorldPosition();

        return new(world.X, world.Y, Size.X, Size.Y);
    }

    /// <summary>
    /// Gets the world position based on parent and anchor.
    /// </summary>
    public Vector2 GetWorldPosition()
    {
        var parentPosition = Parent?.GetWorldPosition() ?? Vector2.Zero;
        var parentSize = Parent?.Size ?? Vector2.Zero;

        var anchorOffset = Anchor switch
        {
            UIAnchor.TopLeft     => Vector2.Zero,
            UIAnchor.TopRight    => new(parentSize.X - Size.X, 0f),
            UIAnchor.BottomLeft  => new(0f, parentSize.Y - Size.Y),
            UIAnchor.BottomRight => new(parentSize.X - Size.X, parentSize.Y - Size.Y),
            UIAnchor.Center      => new((parentSize.X - Size.X) * 0.5f, (parentSize.Y - Size.Y) * 0.5f),
            _                    => Vector2.Zero
        };

        return parentPosition + anchorOffset + Position;
    }

    /// <summary>
    /// Handles mouse down for this control.
    /// </summary>
    public virtual bool HandleMouseDown(Vector2 point)
        => OnMouseDown?.Invoke(point) ?? false;

    /// <summary>
    /// Handles mouse move for this control.
    /// </summary>
    public virtual bool HandleMouseMove(Vector2 point)
        => OnMouseMove?.Invoke(point) ?? false;

    /// <summary>
    /// Handles mouse up for this control.
    /// </summary>
    public virtual bool HandleMouseUp(Vector2 point)
        => OnMouseUp?.Invoke(point) ?? false;

    /// <summary>
    /// Renders the control.
    /// </summary>
    public virtual void Render(SpriteBatch? spriteBatch, EngineRenderContext? renderContext) { }
}
