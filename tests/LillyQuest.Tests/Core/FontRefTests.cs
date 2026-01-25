using LillyQuest.Core.Graphics.Text;

namespace LillyQuest.Tests.Core;

public class FontRefTests
{
    [Test]
    public void FontRef_Stores_Data()
    {
        var fontRef = new FontRef("default_font", 14, FontKind.TrueType);

        Assert.That(fontRef.Name, Is.EqualTo("default_font"));
        Assert.That(fontRef.Size, Is.EqualTo(14));
        Assert.That(fontRef.Kind, Is.EqualTo(FontKind.TrueType));
    }
}
