using System.Numerics;
using LillyQuest.Engine.Screens.UI;

namespace LillyQuest.Tests.Engine.UI;

public class UIStackPanelTests
{
    [Test]
    public void HorizontalLayout_Centers_On_CrossAxis()
    {
        var panel = new UIStackPanel
        {
            Size = new(100, 50),
            Orientation = UIStackOrientation.Horizontal,
            Spacing = 5f,
            Padding = new(10, 5, 10, 5),
            CrossAxisAlignment = UICrossAlignment.Center
        };

        var a = new UIScreenControl { Size = new(20, 10) };
        panel.Add(a);

        var availableHeight = 50 - 5 - 5;
        var expectedY = 5 + (availableHeight - 10) * 0.5f;

        Assert.That(a.Position, Is.EqualTo(new Vector2(10, expectedY)));
    }

    [Test]
    public void VerticalLayout_Right_Aligns_Children()
    {
        var panel = new UIStackPanel
        {
            Size = new(100, 50),
            Orientation = UIStackOrientation.Vertical,
            Padding = new(10, 5, 10, 5),
            CrossAxisAlignment = UICrossAlignment.Right
        };

        var a = new UIScreenControl { Size = new(20, 10) };
        panel.Add(a);

        var availableWidth = 100 - 10 - 10;
        var expectedX = 10 + (availableWidth - 20);

        Assert.That(a.Position, Is.EqualTo(new Vector2(expectedX, 5)));
    }

    [Test]
    public void VerticalLayout_Uses_Padding_And_Spacing()
    {
        var panel = new UIStackPanel
        {
            Size = new(100, 100),
            Orientation = UIStackOrientation.Vertical,
            Spacing = 4f,
            Padding = new(10, 5, 10, 5),
            CrossAxisAlignment = UICrossAlignment.Left
        };

        var a = new UIScreenControl { Size = new(20, 10) };
        var b = new UIScreenControl { Size = new(20, 10) };
        panel.Add(a);
        panel.Add(b);

        Assert.That(a.Position, Is.EqualTo(new Vector2(10, 5)));
        Assert.That(b.Position, Is.EqualTo(new Vector2(10, 5 + 10 + 4)));
    }
}
