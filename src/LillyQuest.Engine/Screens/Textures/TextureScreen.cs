using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Managers.Screens.Base;

namespace LillyQuest.Engine.Screens.Textures;

public class TextureScreen : BaseScreen
{
    private readonly ITextureManager _textureManager;

    public string TextureName { get; set; } = string.Empty;
    public LyColor Tint { get; set; } = LyColor.White;

    public TextureScreen(ITextureManager textureManager)
        => _textureManager = textureManager ?? throw new ArgumentNullException(nameof(textureManager));

    public override void Render(SpriteBatch spriteBatch, EngineRenderContext renderContext)
    {
        if (string.IsNullOrWhiteSpace(TextureName))
        {
            return;
        }

        var textureSize = GetTextureSize();
        if (textureSize.X <= 0f || textureSize.Y <= 0f)
        {
            return;
        }

        var scissorX = (int)(Position.X + Margin.X);
        var scissorY = (int)(Position.Y + Margin.Y);
        var scissorWidth = (int)(Size.X - Margin.X - Margin.Z);
        var scissorHeight = (int)(Size.Y - Margin.Y - Margin.W);

        scissorWidth = Math.Max(0, scissorWidth);
        scissorHeight = Math.Max(0, scissorHeight);

        spriteBatch.SetScissor(scissorX, scissorY, scissorWidth, scissorHeight);
        spriteBatch.PushTranslation(Position);

        var placement = ComputeTexturePlacement(textureSize);
        spriteBatch.DrawTexture(TextureName, placement.position, placement.size, Tint);

        spriteBatch.PopTranslation();
        spriteBatch.DisableScissor();
    }

    protected virtual Vector2 GetTextureSize()
    {
        if (!_textureManager.TryGetTexture(TextureName, out var texture))
        {
            return Vector2.Zero;
        }

        return new Vector2(texture.Width, texture.Height);
    }

    protected (Vector2 position, Vector2 size) ComputeTexturePlacement(Vector2 textureSize)
    {
        var position = (Size - textureSize) * 0.5f;

        return (position, textureSize);
    }
}
