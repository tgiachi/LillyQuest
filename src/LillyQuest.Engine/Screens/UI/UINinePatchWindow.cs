using System.Numerics;
using LillyQuest.Core.Data.Assets;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using Silk.NET.Maths;

namespace LillyQuest.Engine.Screens.UI;

public sealed class UINinePatchWindow : UIWindow
{
    private readonly INineSliceAssetManager _nineSliceManager;
    private readonly ITextureManager _textureManager;

    public string NineSliceKey { get; set; } = string.Empty;
    public LyColor BorderTint { get; set; } = LyColor.White;
    public LyColor CenterTint { get; set; } = LyColor.White;
    public Vector4D<float> TitleMargin { get; set; } = Vector4D<float>.Zero;
    public Vector4D<float> ContentMargin { get; set; } = Vector4D<float>.Zero;
    public float NineSliceScale { get; set; } = 1f;

    public UINinePatchWindow(INineSliceAssetManager nineSliceManager, ITextureManager textureManager)
    {
        _nineSliceManager = nineSliceManager;
        _textureManager = textureManager;
    }

    public override Vector2 GetContentOrigin()
    {
        var world = GetWorldPosition();

        return new(world.X + ContentMargin.X, world.Y + ContentMargin.Y);
    }

    public override Vector2 GetTitlePosition()
    {
        var world = GetWorldPosition();

        return new(world.X + TitleMargin.X, world.Y + TitleMargin.Y);
    }

    protected override Vector2 GetContentOffset()
        => new(ContentMargin.X, ContentMargin.Y);

    protected override void RenderBackground(SpriteBatch spriteBatch, EngineRenderContext renderContext)
    {
        if (string.IsNullOrWhiteSpace(NineSliceKey))
        {
            return;
        }

        if (!_nineSliceManager.TryGetNineSlice(NineSliceKey, out var slice))
        {
            return;
        }

        if (!_textureManager.TryGetTexture(slice.TextureName, out var texture))
        {
            return;
        }

        DrawNineSlice(spriteBatch, texture, slice);
    }

    private void DrawNineSlice(SpriteBatch spriteBatch, Texture2D texture, NineSliceDefinition slice)
    {
        var world = GetWorldPosition();

        var leftWidth = slice.Left.Size.X * NineSliceScale;
        var rightWidth = slice.Right.Size.X * NineSliceScale;
        var topHeight = slice.Top.Size.Y * NineSliceScale;
        var bottomHeight = slice.Bottom.Size.Y * NineSliceScale;

        var centerWidth = MathF.Max(0f, Size.X - leftWidth - rightWidth);
        var centerHeight = MathF.Max(0f, Size.Y - topHeight - bottomHeight);

        DrawSlice(spriteBatch, texture, world, new(leftWidth, topHeight), slice.TopLeft, BorderTint);
        DrawSlice(
            spriteBatch,
            texture,
            new(world.X + leftWidth + centerWidth, world.Y),
            new(rightWidth, topHeight),
            slice.TopRight,
            BorderTint
        );
        DrawSlice(
            spriteBatch,
            texture,
            new(world.X, world.Y + topHeight + centerHeight),
            new(leftWidth, bottomHeight),
            slice.BottomLeft,
            BorderTint
        );
        DrawSlice(
            spriteBatch,
            texture,
            new(world.X + leftWidth + centerWidth, world.Y + topHeight + centerHeight),
            new(rightWidth, bottomHeight),
            slice.BottomRight,
            BorderTint
        );

        DrawTiled(
            spriteBatch,
            texture,
            new(world.X + leftWidth, world.Y),
            new(centerWidth, topHeight),
            slice.Top,
            BorderTint
        );
        DrawTiled(
            spriteBatch,
            texture,
            new(world.X + leftWidth, world.Y + topHeight + centerHeight),
            new(centerWidth, bottomHeight),
            slice.Bottom,
            BorderTint
        );
        DrawTiled(
            spriteBatch,
            texture,
            new(world.X, world.Y + topHeight),
            new(leftWidth, centerHeight),
            slice.Left,
            BorderTint
        );
        DrawTiled(
            spriteBatch,
            texture,
            new(world.X + leftWidth + centerWidth, world.Y + topHeight),
            new(rightWidth, centerHeight),
            slice.Right,
            BorderTint
        );

        DrawTiled(
            spriteBatch,
            texture,
            new(world.X + leftWidth, world.Y + topHeight),
            new(centerWidth, centerHeight),
            slice.Center,
            CenterTint
        );
    }

    private void DrawSlice(
        SpriteBatch spriteBatch,
        Texture2D texture,
        Vector2 position,
        Vector2 size,
        Rectangle<int> sourceRect,
        LyColor tint
    )
    {
        if (size.X <= 0f || size.Y <= 0f)
        {
            return;
        }

        var uv = ToUvRect(texture, sourceRect, 1f, 1f);
        spriteBatch.Draw(texture, position, size, tint, 0f, Vector2.Zero, uv, 0f);
    }

    private void DrawTiled(
        SpriteBatch spriteBatch,
        Texture2D texture,
        Vector2 position,
        Vector2 size,
        Rectangle<int> sourceRect,
        LyColor tint
    )
    {
        if (size.X <= 0f || size.Y <= 0f)
        {
            return;
        }

        var tileWidth = sourceRect.Size.X * NineSliceScale;
        var tileHeight = sourceRect.Size.Y * NineSliceScale;

        if (tileWidth <= 0f || tileHeight <= 0f)
        {
            return;
        }

        for (var y = 0f; y < size.Y; y += tileHeight)
        {
            var drawHeight = MathF.Min(tileHeight, size.Y - y);
            var vScale = drawHeight / tileHeight;

            for (var x = 0f; x < size.X; x += tileWidth)
            {
                var drawWidth = MathF.Min(tileWidth, size.X - x);
                var uScale = drawWidth / tileWidth;
                var uv = ToUvRect(texture, sourceRect, uScale, vScale);
                spriteBatch.Draw(
                    texture,
                    new(position.X + x, position.Y + y),
                    new(drawWidth, drawHeight),
                    tint,
                    0f,
                    Vector2.Zero,
                    uv,
                    0f
                );
            }
        }
    }

    private static Rectangle<float> ToUvRect(Texture2D texture, Rectangle<int> sourceRect, float uScale, float vScale)
    {
        var u = (float)sourceRect.Origin.X / texture.Width;
        var v = (float)sourceRect.Origin.Y / texture.Height;
        var width = sourceRect.Size.X * uScale / texture.Width;
        var height = sourceRect.Size.Y * vScale / texture.Height;

        return new(u, v, width, height);
    }
}
