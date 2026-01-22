namespace LillyQuest.Engine.Screens.UI;

/// <summary>
/// Manages focus for UI controls.
/// </summary>
public sealed class UIFocusManager
{
    public UIScreenControl? Focused { get; private set; }

    public void FocusNext(UIScreenRoot root)
    {
        var focusables = root.Children.Where(child => child.IsFocusable).ToList();

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
        var focusables = root.Children.Where(child => child.IsFocusable).ToList();

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
}
