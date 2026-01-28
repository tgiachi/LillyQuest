using System.Numerics;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Engine.Screens.UI;

public sealed class UIStackPanel : UIScreenControl
{
    private readonly List<UIScreenControl> _children = [];
    private UIStackOrientation _orientation = UIStackOrientation.Vertical;
    private UICrossAlignment _crossAxisAlignment = UICrossAlignment.Left;
    private Vector4 _padding;
    private float _spacing;

    public IReadOnlyList<UIScreenControl> Children => _children;

    public UIStackOrientation Orientation
    {
        get => _orientation;
        set
        {
            _orientation = value;
            ApplyLayout();
        }
    }

    public UICrossAlignment CrossAxisAlignment
    {
        get => _crossAxisAlignment;
        set
        {
            _crossAxisAlignment = value;
            ApplyLayout();
        }
    }

    public Vector4 Padding
    {
        get => _padding;
        set
        {
            _padding = value;
            ApplyLayout();
        }
    }

    public float Spacing
    {
        get => _spacing;
        set
        {
            _spacing = value;
            ApplyLayout();
        }
    }

    public void Add(UIScreenControl control)
    {
        if (control == null)
        {
            return;
        }

        control.Parent = this;
        _children.Add(control);
        ApplyLayout();
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
        ApplyLayout();
    }

    public override void Update(GameTime gameTime)
    {
        foreach (var child in _children)
        {
            child.Update(gameTime);
        }
    }

    private void ApplyLayout()
    {
        var cursor = _orientation == UIStackOrientation.Vertical ? _padding.Y : _padding.X;

        foreach (var child in _children)
        {
            if (_orientation == UIStackOrientation.Vertical)
            {
                var x = ComputeCrossAxisPosition(Size.X, child.Size.X, _padding.X, _padding.Z);
                child.Position = new(x, cursor);
                cursor += child.Size.Y + _spacing;
            }
            else
            {
                var y = ComputeCrossAxisPosition(Size.Y, child.Size.Y, _padding.Y, _padding.W);
                child.Position = new(cursor, y);
                cursor += child.Size.X + _spacing;
            }
        }
    }

    private float ComputeCrossAxisPosition(float total, float child, float startPad, float endPad)
    {
        var available = total - startPad - endPad;

        return _crossAxisAlignment switch
        {
            UICrossAlignment.Left   => startPad,
            UICrossAlignment.Center => startPad + (available - child) * 0.5f,
            UICrossAlignment.Right  => startPad + (available - child),
            _                       => startPad
        };
    }
}
