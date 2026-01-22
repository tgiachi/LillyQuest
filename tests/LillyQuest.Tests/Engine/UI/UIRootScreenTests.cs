using System.Numerics;
using LillyQuest.Engine.Screens.UI;
using Silk.NET.Input;

namespace LillyQuest.Tests.Engine.UI;

public class UIRootScreenTests
{
    [Test]
    public void MouseDown_Captures_Control_And_Forwards_Move_Up()
    {
        var root = new UIRootScreen();
        var control = new UIScreenControl
        {
            Position = Vector2.Zero,
            Size = new(20, 20)
        };
        var moves = 0;
        var ups = 0;

        control.OnMouseDown = _ => true;
        control.OnMouseMove = _ =>
                              {
                                  moves++;

                                  return true;
                              };
        control.OnMouseUp = _ =>
                            {
                                ups++;

                                return true;
                            };
        root.Root.Add(control);

        Assert.That(root.OnMouseDown(5, 5, Array.Empty<MouseButton>()), Is.True);
        Assert.That(root.OnMouseMove(10, 10), Is.True);
        Assert.That(root.OnMouseUp(10, 10, Array.Empty<MouseButton>()), Is.True);
        Assert.That(moves, Is.EqualTo(1));
        Assert.That(ups, Is.EqualTo(1));
    }
}
