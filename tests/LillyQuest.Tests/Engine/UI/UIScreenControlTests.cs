using System.Numerics;
using LillyQuest.Engine.Screens.UI;
using NUnit.Framework;

namespace LillyQuest.Tests.Engine.UI;

public class UIScreenControlTests
{
    [Test]
    public void GetWorldPosition_UsesParentAndAnchor()
    {
        var parent = new UIScreenControl
        {
            Position = new Vector2(10, 10),
            Size = new Vector2(100, 100)
        };
        var child = new UIScreenControl
        {
            Position = new Vector2(5, 5),
            Size = new Vector2(10, 10),
            Anchor = UIAnchor.TopLeft,
            Parent = parent
        };

        Assert.That(child.GetWorldPosition(), Is.EqualTo(new Vector2(15, 15)));
    }
}
