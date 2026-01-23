using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace LillyQuest.Engine.Screens.UI;

/// <summary>
/// Base UI control for pixel-based UI.
/// </summary>
public class UIScreenControl
{
    private readonly List<UIScreenControl> _children = [];

    public IReadOnlyList<UIScreenControl> Children => _children;

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
    public Func<Vector2, float, bool>? OnMouseWheel { get; set; }
    public Func<Vector2, IReadOnlyList<MouseButton>, bool>? OnMouseDownWithButtons { get; set; }
    public Func<Vector2, IReadOnlyList<MouseButton>, bool>? OnMouseUpWithButtons { get; set; }

    public void AddChild(UIScreenControl control)
    {
        if (control == null)
        {
            return;
        }

        control.Parent = this;
        _children.Add(control);
    }

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
    /// Handles mouse down for this control with button information.
    /// </summary>
    public virtual bool HandleMouseDown(Vector2 point, IReadOnlyList<MouseButton> buttons)
        => OnMouseDownWithButtons?.Invoke(point, buttons) == true || HandleMouseDown(point);

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
    /// Handles mouse up for this control with button information.
    /// </summary>
    public virtual bool HandleMouseUp(Vector2 point, IReadOnlyList<MouseButton> buttons)
        => OnMouseUpWithButtons?.Invoke(point, buttons) == true || HandleMouseUp(point);

    /// <summary>
    /// Handles mouse wheel input for this control.
    /// </summary>
    public virtual bool HandleMouseWheel(Vector2 point, float delta)
        => OnMouseWheel?.Invoke(point, delta) ?? false;

    public void RemoveChild(UIScreenControl control)
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

    /// <summary>
    /// Renders the control.
    /// </summary>
    public virtual void Render(SpriteBatch? spriteBatch, EngineRenderContext? renderContext) { }

    /// <summary>
    /// Updates the control.
    /// </summary>
    public virtual void Update(GameTime gameTime) { }
}
