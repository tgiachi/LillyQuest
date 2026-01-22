using Silk.NET.Maths;

namespace LillyQuest.Core.Data.Assets;

public readonly struct NineSliceDefinition
{
    public string TextureName { get; }

    public Rectangle<int> TopLeft { get; }
    public Rectangle<int> Top { get; }
    public Rectangle<int> TopRight { get; }
    public Rectangle<int> Left { get; }
    public Rectangle<int> Center { get; }
    public Rectangle<int> Right { get; }
    public Rectangle<int> BottomLeft { get; }
    public Rectangle<int> Bottom { get; }
    public Rectangle<int> BottomRight { get; }

    public NineSliceDefinition(
        string textureName,
        Rectangle<int> topLeft,
        Rectangle<int> top,
        Rectangle<int> topRight,
        Rectangle<int> left,
        Rectangle<int> center,
        Rectangle<int> right,
        Rectangle<int> bottomLeft,
        Rectangle<int> bottom,
        Rectangle<int> bottomRight
    )
    {
        TextureName = textureName;
        TopLeft = topLeft;
        Top = top;
        TopRight = topRight;
        Left = left;
        Center = center;
        Right = right;
        BottomLeft = bottomLeft;
        Bottom = bottom;
        BottomRight = bottomRight;
    }
}
