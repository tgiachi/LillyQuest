using System.Numerics;
using FontStashSharp;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Core.Graphics.Text;

public sealed class TrueTypeFontHandle : IFontHandle
{
    private readonly DynamicSpriteFont _font;

    public TrueTypeFontHandle(DynamicSpriteFont font)
        => _font = font;

    public void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, LyColor color, float depth = 0f)
    {
        var fsColor = new FSColor(color.R, color.G, color.B, color.A);
        _font.DrawText(spriteBatch, text, position, fsColor, 0f, Vector2.Zero, Vector2.One * 2f, depth);
    }

    public Vector2 MeasureText(string text)
    {
        var size = _font.MeasureString(text);

        return new Vector2(size.X, size.Y) * 2f;
    }
}
