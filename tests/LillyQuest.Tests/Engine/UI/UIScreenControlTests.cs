using System.Numerics;
using LillyQuest.Engine.Screens.UI;

namespace LillyQuest.Tests.Engine.UI;

public class UIScreenControlTests
{
    [Test]
    public void GetWorldPosition_UsesParentAndAnchor()
    {
        var parent = new UIScreenControl
        {
            Position = new(10, 10),
            Size = new(100, 100)
        };
        var child = new UIScreenControl
        {
            Position = new(5, 5),
            Size = new(10, 10),
            Anchor = UIAnchor.TopLeft,
            Parent = parent
        };

        Assert.That(child.GetWorldPosition(), Is.EqualTo(new Vector2(15, 15)));
    }

    [Test]
    public void Add_Remove_Manage_Children()
    {
        var parent = new UIScreenControl();
        var child = new UIScreenControl();

        parent.AddChild(child);
        Assert.That(parent.Children.Count, Is.EqualTo(1));
        Assert.That(child.Parent, Is.EqualTo(parent));

        parent.RemoveChild(child);
        Assert.That(parent.Children.Count, Is.EqualTo(0));
        Assert.That(child.Parent, Is.Null);
    }
}
