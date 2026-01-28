using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Data.Input;
using Silk.NET.Input;

namespace LillyQuest.Engine.Screens.UI;

public sealed class UIMenu : UIScreenControl
{
    private readonly List<MenuItem> _items = [];

    public IReadOnlyList<MenuItem> Items => _items;

    public int SelectedIndex { get; private set; } = -1;
    public int HoveredIndex { get; private set; } = -1;
    public int PressedIndex { get; private set; } = -1;

    public float ItemHeight { get; set; } = 24f;
    public float ItemSpacing { get; set; } = 2f;
    public Vector4 Padding { get; set; } = Vector4.Zero;

    public FontRef Font { get; set; } = new("default_font", 14, FontKind.TrueType);
    public LyColor TextColor { get; set; } = LyColor.White;
    public LyColor HoveredColor { get; set; } = LyColor.White;
    public LyColor PressedColor { get; set; } = LyColor.White;
    public LyColor SelectedColor { get; set; } = LyColor.White;
    public LyColor DisabledColor { get; set; } = LyColor.Gray;

    public UIMenu()
        => IsFocusable = true;

    public void SetItems(IEnumerable<MenuItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
        SelectFirstEnabled();
        HoveredIndex = -1;
        PressedIndex = -1;
    }

    public override bool HandleKeyPress(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        if (!IsEnabled || !IsVisible || _items.Count == 0)
        {
            return false;
        }

        if (keys.Contains(Key.Up) || keys.Contains(Key.W))
        {
            MoveSelection(-1);
            return true;
        }

        if (keys.Contains(Key.Down) || keys.Contains(Key.S))
        {
            MoveSelection(1);
            return true;
        }

        if (keys.Contains(Key.Enter))
        {
            ActivateSelected();
            return true;
        }

        return false;
    }

    public override bool HandleMouseMove(Vector2 point)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }

        var index = GetItemIndexAtPoint(point);
        if (index != HoveredIndex)
        {
            if (index >= 0 && _items[index].IsEnabled)
            {
                HoveredIndex = index;
                SelectedIndex = index;
            }
            else
            {
                HoveredIndex = -1;
            }
        }

        return index >= 0;
    }

    public override bool HandleMouseDown(Vector2 point)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }

        var index = GetItemIndexAtPoint(point);
        if (index < 0 || !_items[index].IsEnabled)
        {
            return false;
        }

        SelectedIndex = index;
        PressedIndex = index;

        return true;
    }

    public override bool HandleMouseUp(Vector2 point)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }

        var index = GetItemIndexAtPoint(point);
        var handled = index >= 0;

        if (PressedIndex >= 0 && PressedIndex == index && _items[index].IsEnabled)
        {
            _items[index].OnSelect?.Invoke();
            handled = true;
        }

        PressedIndex = -1;

        return handled;
    }

    public override void Render(SpriteBatch? spriteBatch, EngineRenderContext? renderContext)
    {
        if (spriteBatch == null || renderContext == null || _items.Count == 0)
        {
            return;
        }

        var world = GetWorldPosition();
        var y = world.Y + Padding.Y;

        for (var i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            var color = GetItemColor(i, item);
            var textSize = spriteBatch.MeasureText(Font, item.Text);
            var textX = world.X + Padding.X;
            var textY = y + (ItemHeight - textSize.Y) * 0.5f;
            spriteBatch.DrawText(Font, item.Text, new(textX, textY), color);
            y += ItemHeight + ItemSpacing;
        }
    }

    private void SelectFirstEnabled()
    {
        SelectedIndex = _items.FindIndex(item => item.IsEnabled);
    }

    private void MoveSelection(int direction)
    {
        if (_items.Count == 0)
        {
            SelectedIndex = -1;
            return;
        }

        if (SelectedIndex < 0)
        {
            SelectedIndex = _items.FindIndex(item => item.IsEnabled);
            return;
        }

        var index = SelectedIndex;
        for (var i = 0; i < _items.Count; i++)
        {
            index = (index + direction + _items.Count) % _items.Count;
            if (_items[index].IsEnabled)
            {
                SelectedIndex = index;
                return;
            }
        }
    }

    private void ActivateSelected()
    {
        if (SelectedIndex < 0 || SelectedIndex >= _items.Count)
        {
            return;
        }

        var item = _items[SelectedIndex];
        if (item.IsEnabled)
        {
            item.OnSelect?.Invoke();
        }
    }

    private int GetItemIndexAtPoint(Vector2 point)
    {
        var bounds = GetBounds();
        if (point.X < bounds.Origin.X ||
            point.X > bounds.Origin.X + bounds.Size.X ||
            point.Y < bounds.Origin.Y ||
            point.Y > bounds.Origin.Y + bounds.Size.Y)
        {
            return -1;
        }

        var localY = point.Y - bounds.Origin.Y - Padding.Y;
        if (localY < 0f)
        {
            return -1;
        }

        var slotHeight = ItemHeight + ItemSpacing;
        if (slotHeight <= 0f)
        {
            return -1;
        }

        var index = (int)(localY / slotHeight);
        if (index < 0 || index >= _items.Count)
        {
            return -1;
        }

        var insideItem = localY - index * slotHeight <= ItemHeight;
        return insideItem ? index : -1;
    }

    private LyColor GetItemColor(int index, MenuItem item)
    {
        if (!item.IsEnabled)
        {
            return DisabledColor;
        }

        if (index == PressedIndex)
        {
            return PressedColor;
        }

        if (index == HoveredIndex)
        {
            return HoveredColor;
        }

        if (index == SelectedIndex)
        {
            return SelectedColor;
        }

        return TextColor;
    }
}
