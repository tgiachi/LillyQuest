using FontStashSharp.Interfaces;

namespace LillyQuest.Core.Graphics.Rendering2D;

public class SpriteBatchItem
{
    public object Texture { get; }
    public float Depth { get; }
    public VertexPositionColorTexture TopLeft { get; }
    public VertexPositionColorTexture TopRight { get; }
    public VertexPositionColorTexture BottomLeft { get; }
    public VertexPositionColorTexture BottomRight { get; }

    public SpriteBatchItem(
        object texture,
        float depth,
        VertexPositionColorTexture topLeft,
        VertexPositionColorTexture topRight,
        VertexPositionColorTexture bottomLeft,
        VertexPositionColorTexture bottomRight
    )
    {
        Texture = texture;
        Depth = depth;
        TopLeft = topLeft;
        TopRight = topRight;
        BottomLeft = bottomLeft;
        BottomRight = bottomRight;
    }
}
