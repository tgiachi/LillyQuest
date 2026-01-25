using System.Numerics;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Core.Graphics.Text;

public sealed class BitmapFontHandle : IFontHandle
{
    private readonly BitmapFont _font;
    private readonly int _size;

    public BitmapFontHandle(BitmapFont font, int size)
    {
        _font = font;
        _size = size;
    }

    public Vector2 MeasureText(string text)
    {
        var glyphHeight = _size > 0 ? _size : _font.TileHeight;
        var aspect = _font.TileWidth / (float)_font.TileHeight;
        var glyphWidth = _size > 0 ? glyphHeight * aspect : _font.TileWidth;
        var spacingX = _font.Spacing * (glyphWidth / _font.TileWidth);
        var spacingY = _font.Spacing * (glyphHeight / _font.TileHeight);

        var maxWidth = 0f;
        var lineWidth = 0f;
        var lines = 1;

        foreach (var ch in text)
        {
            if (ch == '\r')
            {
                continue;
            }

            if (ch == '\n')
            {
                maxWidth = MathF.Max(maxWidth, lineWidth);
                lineWidth = 0f;
                lines++;
                continue;
            }

            lineWidth += glyphWidth + spacingX;
        }

        maxWidth = MathF.Max(maxWidth, lineWidth);
        var totalHeight = lines * glyphHeight + (lines - 1) * spacingY;

        return new Vector2(maxWidth, totalHeight);
    }

    public void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, LyColor color, float depth = 0f)
        => spriteBatch.DrawTextBitmap(_font, text, position, _size, color, depth);
}
