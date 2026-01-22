using System.Numerics;

namespace LillyQuest.Engine.Screens.UI;

/// <summary>
/// Root container for UI controls.
/// </summary>
public sealed class UIScreenRoot
{
    private readonly List<UIScreenControl> _children = [];

    public IReadOnlyList<UIScreenControl> Children => _children;
    public UIFocusManager FocusManager { get; } = new();

    public void Add(UIScreenControl control)
    {
        if (control == null)
        {
            return;
        }

        _children.Add(control);
    }

    public void Remove(UIScreenControl control)
    {
        if (control == null)
        {
            return;
        }

        _children.Remove(control);
    }

    public void BringToFront(UIScreenControl control)
    {
        if (control == null || !_children.Contains(control))
        {
            return;
        }

        var maxZ = _children.Max(child => child.ZIndex);
        if (control.ZIndex <= maxZ)
        {
            control.ZIndex = maxZ + 1;
        }
    }

    /// <summary>
    /// Returns the top-most control containing the point.
    /// </summary>
    public UIScreenControl? HitTest(Vector2 point)
    {
        foreach (var control in _children
                                .OrderByDescending(child => child.ZIndex)
                                .ThenByDescending(child => _children.IndexOf(child)))
        {
            var bounds = control.GetBounds();

            if (point.X >= bounds.Origin.X &&
                point.X <= bounds.Origin.X + bounds.Size.X &&
                point.Y >= bounds.Origin.Y &&
                point.Y <= bounds.Origin.Y + bounds.Size.Y)
            {
                return control;
            }
        }

        return null;
    }
}
