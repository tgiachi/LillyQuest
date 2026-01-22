using LillyQuest.Engine.Screens.UI;

namespace LillyQuest.Tests.Engine.UI;

public class UILabelTests
{
    [Test]
    public void Render_WithNullSpriteBatch_DoesNotThrow()
    {
        var label = new UILabel
        {
            Text = "Hello"
        };

        Assert.DoesNotThrow(() => label.Render(null, null));
    }
}
