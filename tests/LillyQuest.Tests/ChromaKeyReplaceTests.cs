using System.Reflection;
using LillyQuest.Core.Managers.Assets;

namespace LillyQuest.Tests;

public class ChromaKeyReplaceTests
{
    [Test]
    public void ReplaceChromaKey_MagentaPixels_ClearAlphaAndRgb()
    {
        var pixelData = new byte[]
        {
            255, 0, 255, 255, // magenta
            10, 20, 30, 255   // non-magenta
        };

        var method = typeof(TextureManager).GetMethod(
            "ReplaceChromaKey",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        Assert.That(method, Is.Not.Null);

        method!.Invoke(null, new object[] { pixelData, (byte)0 });

        Assert.That(pixelData[0], Is.EqualTo(0));
        Assert.That(pixelData[1], Is.EqualTo(0));
        Assert.That(pixelData[2], Is.EqualTo(0));
        Assert.That(pixelData[3], Is.EqualTo(0));

        Assert.That(pixelData[4], Is.EqualTo(10));
        Assert.That(pixelData[5], Is.EqualTo(20));
        Assert.That(pixelData[6], Is.EqualTo(30));
        Assert.That(pixelData[7], Is.EqualTo(255));
    }
}
