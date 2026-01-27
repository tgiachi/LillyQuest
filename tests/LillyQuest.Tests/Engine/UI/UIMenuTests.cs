using System.Numerics;
using LillyQuest.Engine.Data.Input;
using LillyQuest.Engine.Screens.UI;
using Silk.NET.Input;

namespace LillyQuest.Tests.Engine.UI;

public class UIMenuTests
{
    [Test]
    public void Menu_Defaults_To_First_Enabled_Item()
    {
        var menu = new UIMenu
        {
            Size = new(200, 200)
        };

        menu.SetItems(new List<MenuItem>
        {
            new("A", () => { }, IsEnabled: false),
            new("B", () => { }, IsEnabled: true),
            new("C", () => { }, IsEnabled: true)
        });

        Assert.That(menu.SelectedIndex, Is.EqualTo(1));
    }

    [Test]
    public void Menu_Keyboard_Navigation_Wraps_And_Skips_Disabled()
    {
        var menu = new UIMenu { Size = new(200, 200) };
        menu.SetItems(new List<MenuItem>
        {
            new("A", () => { }),
            new("B", () => { }, IsEnabled: false),
            new("C", () => { })
        });

        menu.HandleKeyPress(KeyModifierType.None, new[] { Key.Down });
        Assert.That(menu.SelectedIndex, Is.EqualTo(2));

        menu.HandleKeyPress(KeyModifierType.None, new[] { Key.Down });
        Assert.That(menu.SelectedIndex, Is.EqualTo(0));
    }

    [Test]
    public void Menu_Click_Invokes_Selected_Item()
    {
        var invoked = 0;
        var menu = new UIMenu
        {
            Size = new(200, 200),
            ItemHeight = 20,
            ItemSpacing = 0,
            Padding = Vector4.Zero
        };
        menu.SetItems(new List<MenuItem>
        {
            new("A", () => invoked++),
            new("B", () => invoked++)
        });

        menu.HandleMouseMove(new(10, 10));
        menu.HandleMouseDown(new(10, 10));
        menu.HandleMouseUp(new(10, 10));

        Assert.That(invoked, Is.EqualTo(1));
    }
}
