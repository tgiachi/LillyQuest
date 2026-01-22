using LillyQuest.Engine.Screens.UI;

namespace LillyQuest.Tests.Engine.UI;

public class UIFocusManagerTests
{
    [Test]
    public void FocusNext_SkipsNonFocusable()
    {
        var root = new UIScreenRoot();
        var a = new UIScreenControl { IsFocusable = true };
        var b = new UIScreenControl { IsFocusable = false };
        var c = new UIScreenControl { IsFocusable = true };
        root.Add(a);
        root.Add(b);
        root.Add(c);

        root.FocusManager.RequestFocus(a);
        root.FocusManager.FocusNext(root);

        Assert.That(root.FocusManager.Focused, Is.EqualTo(c));
    }
}
