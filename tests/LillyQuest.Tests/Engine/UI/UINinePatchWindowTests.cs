using System.Numerics;
using LillyQuest.Engine.Screens.UI;
using NUnit.Framework;
using Silk.NET.Maths;

namespace LillyQuest.Tests.Engine.UI;

public class UINinePatchWindowTests
{
    [Test]
    public void TitleAndContentMargins_AreApplied()
    {
        var window = new UINinePatchWindow(null!, null!)
        {
            Position = Vector2.Zero,
            Size = new Vector2(100, 60),
            TitleMargin = new Vector4D<float>(6, 4, 0, 0),
            ContentMargin = new Vector4D<float>(8, 10, 0, 0),
            NineSliceScale = 2f
        };

        Assert.That(window.GetTitlePosition(), Is.EqualTo(new Vector2(6, 4)));
        Assert.That(window.GetContentOrigin(), Is.EqualTo(new Vector2(8, 10)));
    }

    [Test]
    public void Render_DoesNotThrow_WhenMissingSpriteBatch()
    {
        var window = new UINinePatchWindow(null!, null!);
        window.Render(null, null);
    }
}
