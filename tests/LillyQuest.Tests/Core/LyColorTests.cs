using LillyQuest.Core.Primitives;

namespace LillyQuest.Tests.Core;

/// <summary>
/// Tests for the LyColor struct
/// </summary>
public class LyColorTests
{
    [Test]
    public void Black_HasCorrectValues()
    {
        var black = LyColor.Black;

        Assert.That(black.A, Is.EqualTo(255));
        Assert.That(black.R, Is.EqualTo(0));
        Assert.That(black.G, Is.EqualTo(0));
        Assert.That(black.B, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_ARGB_SetAllComponents()
    {
        var color = new LyColor(200, 255, 128, 64);

        Assert.That(color.A, Is.EqualTo(200));
        Assert.That(color.R, Is.EqualTo(255));
        Assert.That(color.G, Is.EqualTo(128));
        Assert.That(color.B, Is.EqualTo(64));
    }

    [Test]
    public void Constructor_RGB_DefaultsAlphaTo255()
    {
        var color = new LyColor(255, 128, 64);

        Assert.That(color.A, Is.EqualTo(255));
        Assert.That(color.R, Is.EqualTo(255));
        Assert.That(color.G, Is.EqualTo(128));
        Assert.That(color.B, Is.EqualTo(64));
    }

    [Test]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        var color1 = new LyColor(255, 100, 150, 200);
        var color2 = new LyColor(255, 100, 150, 201);

        Assert.That(color1.Equals(color2), Is.False);
        Assert.That(color1 == color2, Is.False);
        Assert.That(color1 != color2, Is.True);
    }

    [Test]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        var color1 = new LyColor(255, 100, 150, 200);
        var color2 = new LyColor(255, 100, 150, 200);

        Assert.That(color1.Equals(color2), Is.True);
        Assert.That(color1 == color2, Is.True);
        Assert.That(color1 != color2, Is.False);
    }

    [Test]
    public void FromArgb_WithBoundaryValues_CreatesColorCorrectly()
    {
        var minColor = LyColor.FromArgb(0, 0, 0, 0);
        var maxColor = LyColor.FromArgb(255, 255, 255, 255);

        Assert.That(minColor.A, Is.EqualTo(0));
        Assert.That(maxColor.A, Is.EqualTo(255));
        Assert.That(maxColor.R, Is.EqualTo(255));
    }

    [Test, TestCase(-1, 0, 0, 0), TestCase(256, 0, 0, 0), TestCase(0, -1, 0, 0), TestCase(0, 256, 0, 0),
     TestCase(0, 0, -1, 0), TestCase(0, 0, 256, 0), TestCase(0, 0, 0, -1), TestCase(0, 0, 0, 256)]
    public void FromArgb_WithOutOfRangeValues_ThrowsArgumentOutOfRangeException(int a, int r, int g, int b)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LyColor.FromArgb(a, r, g, b));
    }

    [Test]
    public void FromArgb_WithValidValues_CreatesColorCorrectly()
    {
        var color = LyColor.FromArgb(200, 100, 150, 200);

        Assert.That(color.A, Is.EqualTo(200));
        Assert.That(color.R, Is.EqualTo(100));
        Assert.That(color.G, Is.EqualTo(150));
        Assert.That(color.B, Is.EqualTo(200));
    }

    [Test]
    public void FromRgb_WithValidValues_CreatesColorWithFullAlpha()
    {
        var color = LyColor.FromRgb(100, 150, 200);

        Assert.That(color.A, Is.EqualTo(255));
        Assert.That(color.R, Is.EqualTo(100));
        Assert.That(color.G, Is.EqualTo(150));
        Assert.That(color.B, Is.EqualTo(200));
    }

    [Test]
    public void GetHashCode_WithSameValues_ReturnsSameHashCode()
    {
        var color1 = new LyColor(255, 100, 150, 200);
        var color2 = new LyColor(255, 100, 150, 200);

        Assert.That(color1.GetHashCode(), Is.EqualTo(color2.GetHashCode()));
    }

    [Test, TestCase(nameof(LyColor.Black)), TestCase(nameof(LyColor.White)), TestCase(nameof(LyColor.Red)),
     TestCase(nameof(LyColor.Green)), TestCase(nameof(LyColor.Blue)), TestCase(nameof(LyColor.Transparent))]
    public void PredefinedColors_AreAccessible(string colorName)
    {
        var property = typeof(LyColor).GetProperty(colorName);
        Assert.That(property, Is.Not.Null);
        Assert.That(property.CanRead, Is.True);
    }

    [Test]
    public void ToNormalizedTuple_ReturnsNormalizedValues()
    {
        var color = new LyColor(255, 0, 128, 255);
        var (r, g, b, a) = color.ToNormalizedTuple();

        Assert.That(a, Is.EqualTo(1.0f).Within(0.001f));
        Assert.That(r, Is.EqualTo(0.0f).Within(0.001f));
        Assert.That(g, Is.EqualTo(128 / 255f).Within(0.001f));
        Assert.That(b, Is.EqualTo(1.0f).Within(0.001f));
    }

    [Test]
    public void ToString_ReturnsHexadecimalFormat()
    {
        var color = new LyColor(255, 100, 150, 200);
        var result = color.ToString();

        Assert.That(result, Does.StartWith("#"));
        Assert.That(result, Has.Length.EqualTo(9)); // #AARRGGBB
    }

    [Test]
    public void ToSystemColor_ConvertsProperly()
    {
        var lyColor = new LyColor(200, 100, 150, 200);
        var systemColor = lyColor.ToSystemColor();

        Assert.That(systemColor.A, Is.EqualTo(200));
        Assert.That(systemColor.R, Is.EqualTo(100));
        Assert.That(systemColor.G, Is.EqualTo(150));
        Assert.That(systemColor.B, Is.EqualTo(200));
    }

    [Test]
    public void Transparent_HasZeroAlpha()
    {
        var transparent = LyColor.Transparent;

        Assert.That(transparent.A, Is.EqualTo(0));
    }

    [Test]
    public void White_HasCorrectValues()
    {
        var white = LyColor.White;

        Assert.That(white.A, Is.EqualTo(255));
        Assert.That(white.R, Is.EqualTo(255));
        Assert.That(white.G, Is.EqualTo(255));
        Assert.That(white.B, Is.EqualTo(255));
    }

    [Test]
    public void WithAlpha_ChangesAlphaValue()
    {
        var originalColor = new LyColor(200, 100, 150, 200);
        var newColor = originalColor.WithAlpha(128);

        Assert.That(newColor.A, Is.EqualTo(128));
        Assert.That(newColor.R, Is.EqualTo(100));
        Assert.That(newColor.G, Is.EqualTo(150));
        Assert.That(newColor.B, Is.EqualTo(200));
    }
}
