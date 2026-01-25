using System.Numerics;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Core.Graphics.Text;

public interface IFontHandle
{
    Vector2 MeasureText(string text);
    void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, LyColor color, float depth = 0f);
}
