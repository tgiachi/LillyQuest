using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Logging;

namespace LillyQuest.Tests.Engine.Logging;

public class BBCodeParserTests
{
    [Test]
    public void Parse_Handles_Color_And_Styles()
    {
        var parser = new BBCodeParser();
        var spans = parser.Parse("[color=red]Errore[/color] [b]critico[/b]");

        Assert.That(spans.Count, Is.EqualTo(3));
        Assert.That(spans[0].Text, Is.EqualTo("Errore"));
        Assert.That(spans[0].Foreground, Is.EqualTo(LyColor.Red));
        Assert.That(spans[1].Text, Is.EqualTo(" "));
        Assert.That(spans[2].Text, Is.EqualTo("critico"));
        Assert.That(spans[2].Bold, Is.True);
    }
}
