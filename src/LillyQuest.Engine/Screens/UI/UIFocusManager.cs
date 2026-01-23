using System;
using System.Collections.Generic;

namespace LillyQuest.Engine.Screens.UI;

/// <summary>
/// Manages focus for UI controls.
/// </summary>
public sealed class UIFocusManager
{
    public UIScreenControl? Focused { get; private set; }

    public void FocusNext(UIScreenRoot root)
    {
        var focusables = GetFocusables(root);

        if (focusables.Count == 0)
        {
            Focused = null;

            return;
        }

        var currentIndex = Focused != null ? focusables.IndexOf(Focused) : -1;
        var nextIndex = (currentIndex + 1) % focusables.Count;
        Focused = focusables[nextIndex];
    }

    public void FocusPrev(UIScreenRoot root)
    {
        var focusables = GetFocusables(root);

        if (focusables.Count == 0)
        {
            Focused = null;

            return;
        }

        var currentIndex = Focused != null ? focusables.IndexOf(Focused) : 0;
        var prevIndex = (currentIndex - 1 + focusables.Count) % focusables.Count;
        Focused = focusables[prevIndex];
    }

    public void RequestFocus(UIScreenControl control)
    {
        Focused = control;
    }

    private static List<UIScreenControl> GetFocusables(UIScreenRoot root)
    {
        var focusables = new List<UIScreenControl>();

        foreach (var control in root.Children)
        {
            CollectFocusables(control, focusables);
        }

        return focusables;
    }

    private static void CollectFocusables(UIScreenControl control, List<UIScreenControl> focusables)
    {
        if (control.IsFocusable)
        {
            focusables.Add(control);
        }

        foreach (var child in GetChildren(control))
        {
            CollectFocusables(child, focusables);
        }
    }

    private static IReadOnlyList<UIScreenControl> GetChildren(UIScreenControl control)
    {
        return control switch
        {
            UIWindow window => window.Children,
            UIScrollContent scroll => scroll.Children,
            UIButton button => button.Children,
            _ => Array.Empty<UIScreenControl>()
        };
    }
}
