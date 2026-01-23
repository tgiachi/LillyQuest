using System.Drawing;
using System.Globalization;

namespace LillyQuest.Core.Primitives;

public readonly struct LyColor : IEquatable<LyColor>
{
    public byte A { get; }
    public byte R { get; }
    public byte G { get; }
    public byte B { get; }

    public LyColor(byte r, byte g, byte b)
        : this(255, r, g, b) { }

    public LyColor(byte a, byte r, byte g, byte b)
    {
        A = a;
        R = r;
        G = g;
        B = b;
    }

    public static LyColor ActiveBorder => FromArgb(255, 212, 208, 200);
    public static LyColor ActiveCaption => FromArgb(255, 0, 84, 227);
    public static LyColor ActiveCaptionText => FromArgb(255, 255, 255, 255);
    public static LyColor AliceBlue => FromArgb(255, 240, 248, 255);
    public static LyColor AntiqueWhite => FromArgb(255, 250, 235, 215);
    public static LyColor AppWorkspace => FromArgb(255, 128, 128, 128);
    public static LyColor Aqua => FromArgb(255, 0, 255, 255);
    public static LyColor Aquamarine => FromArgb(255, 127, 255, 212);
    public static LyColor Azure => FromArgb(255, 240, 255, 255);
    public static LyColor Beige => FromArgb(255, 245, 245, 220);
    public static LyColor Bisque => FromArgb(255, 255, 228, 196);
    public static LyColor Black => FromArgb(255, 0, 0, 0);
    public static LyColor BlanchedAlmond => FromArgb(255, 255, 235, 205);
    public static LyColor Blue => FromArgb(255, 0, 0, 255);
    public static LyColor BlueViolet => FromArgb(255, 138, 43, 226);
    public static LyColor Brown => FromArgb(255, 165, 42, 42);
    public static LyColor BurlyWood => FromArgb(255, 222, 184, 135);
    public static LyColor ButtonFace => FromArgb(255, 240, 240, 240);
    public static LyColor ButtonHighlight => FromArgb(255, 255, 255, 255);
    public static LyColor ButtonShadow => FromArgb(255, 160, 160, 160);
    public static LyColor CadetBlue => FromArgb(255, 95, 158, 160);
    public static LyColor Chartreuse => FromArgb(255, 127, 255, 0);
    public static LyColor Chocolate => FromArgb(255, 210, 105, 30);
    public static LyColor Control => FromArgb(255, 236, 233, 216);
    public static LyColor ControlLight => FromArgb(255, 241, 239, 226);
    public static LyColor ControlLightLight => FromArgb(255, 255, 255, 255);
    public static LyColor ControlText => FromArgb(255, 0, 0, 0);
    public static LyColor Coral => FromArgb(255, 255, 127, 80);
    public static LyColor CornflowerBlue => FromArgb(255, 100, 149, 237);
    public static LyColor Cornsilk => FromArgb(255, 255, 248, 220);
    public static LyColor Crimson => FromArgb(255, 220, 20, 60);
    public static LyColor Cyan => FromArgb(255, 0, 255, 255);
    public static LyColor Goldenrod => FromArgb(255, 184, 134, 11);
    public static LyColor Gray => FromArgb(255, 169, 169, 169);
    public static LyColor Green => FromArgb(255, 0, 100, 0);
    public static LyColor Khaki => FromArgb(255, 189, 183, 107);
    public static LyColor Magenta => FromArgb(255, 139, 0, 139);
    public static LyColor OliveGreen => FromArgb(255, 85, 107, 47);
    public static LyColor Orange => FromArgb(255, 255, 140, 0);
    public static LyColor Orchid => FromArgb(255, 153, 50, 204);
    public static LyColor Red => FromArgb(255, 139, 0, 0);
    public static LyColor Salmon => FromArgb(255, 233, 150, 122);
    public static LyColor SeaGreen => FromArgb(255, 143, 188, 143);
    public static LyColor SlateBlue => FromArgb(255, 72, 61, 139);
    public static LyColor SlateGray => FromArgb(255, 47, 79, 79);
    public static LyColor Turquoise => FromArgb(255, 0, 206, 209);
    public static LyColor Violet => FromArgb(255, 148, 0, 211);
    public static LyColor DeepPink => FromArgb(255, 255, 20, 147);
    public static LyColor DeepSkyBlue => FromArgb(255, 0, 191, 255);
    public static LyColor Desktop => FromArgb(255, 0, 78, 152);
    public static LyColor DimGray => FromArgb(255, 105, 105, 105);
    public static LyColor DodgerBlue => FromArgb(255, 30, 144, 255);
    public static LyColor Firebrick => FromArgb(255, 178, 34, 34);
    public static LyColor FloralWhite => FromArgb(255, 255, 250, 240);
    public static LyColor ForestGreen => FromArgb(255, 34, 139, 34);
    public static LyColor Fuchsia => FromArgb(255, 255, 0, 255);
    public static LyColor Gainsboro => FromArgb(255, 220, 220, 220);
    public static LyColor GhostWhite => FromArgb(255, 248, 248, 255);
    public static LyColor Gold => FromArgb(255, 255, 215, 0);
    public static LyColor GradientActiveCaption => FromArgb(255, 185, 209, 234);
    public static LyColor GradientInactiveCaption => FromArgb(255, 215, 228, 242);
    public static LyColor GrayText => FromArgb(255, 172, 168, 153);
    public static LyColor GreenYellow => FromArgb(255, 173, 255, 47);
    public static LyColor Highlight => FromArgb(255, 49, 106, 197);
    public static LyColor HighlightText => FromArgb(255, 255, 255, 255);
    public static LyColor Honeydew => FromArgb(255, 240, 255, 240);
    public static LyColor HotPink => FromArgb(255, 255, 105, 180);
    public static LyColor HotTrack => FromArgb(255, 0, 0, 128);
    public static LyColor InactiveBorder => FromArgb(255, 212, 208, 200);
    public static LyColor InactiveCaption => FromArgb(255, 122, 150, 223);
    public static LyColor InactiveCaptionText => FromArgb(255, 216, 228, 248);
    public static LyColor IndianRed => FromArgb(255, 205, 92, 92);
    public static LyColor Indigo => FromArgb(255, 75, 0, 130);
    public static LyColor Info => FromArgb(255, 255, 255, 225);
    public static LyColor InfoText => FromArgb(255, 0, 0, 0);
    public static LyColor Ivory => FromArgb(255, 255, 255, 240);
    public static LyColor Lavender => FromArgb(255, 230, 230, 250);
    public static LyColor LavenderBlush => FromArgb(255, 255, 240, 245);
    public static LyColor LawnGreen => FromArgb(255, 124, 252, 0);
    public static LyColor LemonChiffon => FromArgb(255, 255, 250, 205);
    public static LyColor LightBlue => FromArgb(255, 173, 216, 230);
    public static LyColor LightCoral => FromArgb(255, 240, 128, 128);
    public static LyColor LightCyan => FromArgb(255, 224, 255, 255);
    public static LyColor LightGoldenrodYellow => FromArgb(255, 250, 250, 210);
    public static LyColor LightGray => FromArgb(255, 211, 211, 211);
    public static LyColor LightGreen => FromArgb(255, 144, 238, 144);
    public static LyColor LightPink => FromArgb(255, 255, 182, 193);
    public static LyColor LightSalmon => FromArgb(255, 255, 160, 122);
    public static LyColor LightSeaGreen => FromArgb(255, 32, 178, 170);
    public static LyColor LightSkyBlue => FromArgb(255, 135, 206, 250);
    public static LyColor LightSlateGray => FromArgb(255, 119, 136, 153);
    public static LyColor LightSteelBlue => FromArgb(255, 176, 196, 222);
    public static LyColor LightYellow => FromArgb(255, 255, 255, 224);
    public static LyColor Lime => FromArgb(255, 0, 255, 0);
    public static LyColor LimeGreen => FromArgb(255, 50, 205, 50);
    public static LyColor Linen => FromArgb(255, 250, 240, 230);
    public static LyColor Maroon => FromArgb(255, 128, 0, 0);
    public static LyColor MediumAquamarine => FromArgb(255, 102, 205, 170);
    public static LyColor MediumBlue => FromArgb(255, 0, 0, 205);
    public static LyColor MediumOrchid => FromArgb(255, 186, 85, 211);
    public static LyColor MediumPurple => FromArgb(255, 147, 112, 219);
    public static LyColor MediumSeaGreen => FromArgb(255, 60, 179, 113);
    public static LyColor MediumSlateBlue => FromArgb(255, 123, 104, 238);
    public static LyColor MediumSpringGreen => FromArgb(255, 0, 250, 154);
    public static LyColor MediumTurquoise => FromArgb(255, 72, 209, 204);
    public static LyColor MediumVioletRed => FromArgb(255, 199, 21, 133);
    public static LyColor Menu => FromArgb(255, 255, 255, 255);
    public static LyColor MenuBar => FromArgb(255, 240, 240, 240);
    public static LyColor MenuHighlight => FromArgb(255, 51, 153, 255);
    public static LyColor MenuText => FromArgb(255, 0, 0, 0);
    public static LyColor MidnightBlue => FromArgb(255, 25, 25, 112);
    public static LyColor MintCream => FromArgb(255, 245, 255, 250);
    public static LyColor MistyRose => FromArgb(255, 255, 228, 225);
    public static LyColor Moccasin => FromArgb(255, 255, 228, 181);
    public static LyColor NavajoWhite => FromArgb(255, 255, 222, 173);
    public static LyColor Navy => FromArgb(255, 0, 0, 128);
    public static LyColor OldLace => FromArgb(255, 253, 245, 230);
    public static LyColor Olive => FromArgb(255, 128, 128, 0);
    public static LyColor OliveDrab => FromArgb(255, 107, 142, 35);
    public static LyColor OrangeRed => FromArgb(255, 255, 69, 0);
    public static LyColor PaleGoldenrod => FromArgb(255, 238, 232, 170);
    public static LyColor PaleGreen => FromArgb(255, 152, 251, 152);
    public static LyColor PaleTurquoise => FromArgb(255, 175, 238, 238);
    public static LyColor PaleVioletRed => FromArgb(255, 219, 112, 147);
    public static LyColor PapayaWhip => FromArgb(255, 255, 239, 213);
    public static LyColor PeachPuff => FromArgb(255, 255, 218, 185);
    public static LyColor Peru => FromArgb(255, 205, 133, 63);
    public static LyColor Pink => FromArgb(255, 255, 192, 203);
    public static LyColor Plum => FromArgb(255, 221, 160, 221);
    public static LyColor PowderBlue => FromArgb(255, 176, 224, 230);
    public static LyColor Purple => FromArgb(255, 128, 0, 128);
    public static LyColor RebeccaPurple => FromArgb(255, 102, 51, 153);
    public static LyColor RosyBrown => FromArgb(255, 188, 143, 143);
    public static LyColor RoyalBlue => FromArgb(255, 65, 105, 225);
    public static LyColor SaddleBrown => FromArgb(255, 139, 69, 19);
    public static LyColor SandyBrown => FromArgb(255, 244, 164, 96);
    public static LyColor ScrollBar => FromArgb(255, 212, 208, 200);
    public static LyColor SeaShell => FromArgb(255, 255, 245, 238);
    public static LyColor Sienna => FromArgb(255, 160, 82, 45);
    public static LyColor Silver => FromArgb(255, 192, 192, 192);
    public static LyColor SkyBlue => FromArgb(255, 135, 206, 235);
    public static LyColor Snow => FromArgb(255, 255, 250, 250);
    public static LyColor SpringGreen => FromArgb(255, 0, 255, 127);
    public static LyColor SteelBlue => FromArgb(255, 70, 130, 180);
    public static LyColor Tan => FromArgb(255, 210, 180, 140);
    public static LyColor Teal => FromArgb(255, 0, 128, 128);
    public static LyColor Thistle => FromArgb(255, 216, 191, 216);
    public static LyColor Tomato => FromArgb(255, 255, 99, 71);
    public static LyColor Transparent => FromArgb(0, 255, 255, 255);
    public static LyColor Wheat => FromArgb(255, 245, 222, 179);
    public static LyColor White => FromArgb(255, 255, 255, 255);
    public static LyColor WhiteSmoke => FromArgb(255, 245, 245, 245);
    public static LyColor Window => FromArgb(255, 255, 255, 255);
    public static LyColor WindowFrame => FromArgb(255, 0, 0, 0);
    public static LyColor WindowText => FromArgb(255, 0, 0, 0);
    public static LyColor Yellow => FromArgb(255, 255, 255, 0);
    public static LyColor YellowGreen => FromArgb(255, 154, 205, 50);

    public bool Equals(LyColor other)
        => A == other.A && R == other.R && G == other.G && B == other.B;

    public override bool Equals(object? obj)
        => obj is LyColor other && Equals(other);

    public static LyColor FromArgb(int a, int r, int g, int b)
        => new(ToByte(a, nameof(a)), ToByte(r, nameof(r)), ToByte(g, nameof(g)), ToByte(b, nameof(b)));

    public static LyColor FromHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            throw new ArgumentException("Hex color cannot be empty.", nameof(hex));
        }

        var trimmed = hex.Trim();

        if (trimmed.StartsWith('#'))
        {
            trimmed = trimmed[1..];
        }

        if (trimmed.Length != 6 && trimmed.Length != 8)
        {
            throw new ArgumentException("Hex color must be 6 or 8 characters long.", nameof(hex));
        }

        if (!uint.TryParse(trimmed, NumberStyles.HexNumber, null, out var value))
        {
            throw new ArgumentException("Hex color contains invalid characters.", nameof(hex));
        }

        var hasAlpha = trimmed.Length == 8;
        var r = (byte)((value >> (hasAlpha ? 24 : 16)) & 0xFF);
        var g = (byte)((value >> (hasAlpha ? 16 : 8)) & 0xFF);
        var b = (byte)((value >> (hasAlpha ? 8 : 0)) & 0xFF);
        var a = hasAlpha ? (byte)(value & 0xFF) : (byte)255;

        return new(a, r, g, b);
    }

    public static LyColor FromRgb(int r, int g, int b)
        => new(255, ToByte(r, nameof(r)), ToByte(g, nameof(g)), ToByte(b, nameof(b)));

    public override int GetHashCode()
        => HashCode.Combine(A, R, G, B);

    public static bool operator ==(LyColor left, LyColor right)
        => left.Equals(right);

    public static bool operator !=(LyColor left, LyColor right)
        => !left.Equals(right);

    public (float R, float G, float B, float A) ToNormalizedTuple()
        => (R / 255f, G / 255f, B / 255f, A / 255f);

    public override string ToString()
        => $"#{A:X2}{R:X2}{G:X2}{B:X2}";

    public Color ToSystemColor()
        => Color.FromArgb(A, R, G, B);

    public LyColor WithAlpha(byte a)
        => new(a, R, G, B);

    private static byte ToByte(int value, string paramName)
    {
        if (value < 0 || value > 255)
        {
            throw new ArgumentOutOfRangeException(paramName, value, "Value must be between 0 and 255.");
        }

        return (byte)value;
    }
}
