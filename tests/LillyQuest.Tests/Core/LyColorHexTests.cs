using LillyQuest.Core.Primitives;

namespace LillyQuest.Tests.Core;

public class LyColorHexTests
{
    [TestCase("#e8d7b0", 0xE8, 0xD7, 0xB0, 0xFF)]
    [TestCase("e8d7b0", 0xE8, 0xD7, 0xB0, 0xFF)]
    [TestCase("#e8d7b0cc", 0xE8, 0xD7, 0xB0, 0xCC)]
    [TestCase("e8d7b0cc", 0xE8, 0xD7, 0xB0, 0xCC)]
    public void FromHex_ParsesRgbAndRgba(string hex, byte r, byte g, byte b, byte a)
    {
        var color = LyColor.FromHex(hex);

        Assert.That(color.R, Is.EqualTo(r));
        Assert.That(color.G, Is.EqualTo(g));
        Assert.That(color.B, Is.EqualTo(b));
        Assert.That(color.A, Is.EqualTo(a));
    }

    [TestCase("#123")]
    [TestCase("#12345")]
    [TestCase("#1234567")]
    [TestCase("#123456789")]
    [TestCase("gggggg")]
    [TestCase("")]
    public void FromHex_Invalid_Throws(string hex)
    {
        Assert.Throws<ArgumentException>(() => LyColor.FromHex(hex));
    }
}
