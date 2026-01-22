using System.Numerics;
using LillyQuest.Engine.Screens.UI;
using NUnit.Framework;

namespace LillyQuest.Tests.Engine.UI;

public class UIScreenRootTests
{
    [Test]
    public void HitTest_ReturnsTopMostByZIndex()
    {
        var root = new UIScreenRoot();
        var a = new UIScreenControl { Position = Vector2.Zero, Size = new Vector2(10, 10), ZIndex = 0 };
        var b = new UIScreenControl { Position = Vector2.Zero, Size = new Vector2(10, 10), ZIndex = 1 };
        root.Add(a);
        root.Add(b);

        var hit = root.HitTest(new Vector2(5, 5));

        Assert.That(hit, Is.EqualTo(b));
    }
}
