using System.Numerics;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Services;
using LillyQuest.Engine.Particles;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.Engine.Systems;
using Silk.NET.Maths;

namespace LillyQuest.Engine.Services.Rendering;

public sealed class ParticlePixelRenderer : IParticlePixelRenderer
{
    private readonly ParticleSystem _particleSystem;

    public ParticlePixelRenderer(ParticleSystem particleSystem)
        => _particleSystem = particleSystem;

    public static Vector2 ComputeParticleScreenPosition(
        Vector2 particlePosition,
        Vector2 tileSize,
        float tileRenderScale,
        float layerRenderScale,
        Vector2 viewTileOffset,
        Vector2 viewPixelOffset,
        Vector2 layerPixelOffset
    )
    {
        var scaledTileSize = tileSize * tileRenderScale * layerRenderScale;
        var viewOffsetPx = viewTileOffset * scaledTileSize + viewPixelOffset;
        var particlePx = particlePosition * scaledTileSize;

        return particlePx - viewOffsetPx + layerPixelOffset;
    }

    public void Render(SpriteBatch spriteBatch, TilesetSurfaceScreen screen, int layerIndex)
    {
        if (!screen.TryGetLayerTileInfo(layerIndex, out var tileWidth, out var tileHeight, out var layerPixelOffset))
        {
            return;
        }

        if (!screen.TryGetLayerViewOffsets(layerIndex, out var viewTileOffset, out var viewPixelOffset))
        {
            return;
        }

        if (tileWidth <= 0 || tileHeight <= 0 || screen.TileRenderScale <= 0f)
        {
            return;
        }

        if (!screen.TryGetLayerTileset(layerIndex, out var tileset))
        {
            return;
        }

        var tileSize = new Vector2(tileWidth, tileHeight);
        var layerScale = screen.GetLayerRenderScale(layerIndex);
        var tileRenderScale = screen.TileRenderScale;

        var rendered = 0;

        foreach (var particle in _particleSystem.GetVisibleParticles())
        {
            if (particle.TileId < 0 || particle.TileId >= tileset.TileCount)
            {
                continue;
            }

            var particlePosition = ComputeParticleScreenPosition(
                particle.Position,
                tileSize,
                tileRenderScale,
                layerScale,
                viewTileOffset,
                viewPixelOffset,
                layerPixelOffset
            );

            var foreground = particle.ForegroundColor;
            var background = particle.BackgroundColor;

            if (particle.Flags.HasFlag(ParticleFlags.FadeOut))
            {
                var normalizedLife = Math.Clamp(particle.Lifetime / 2f, 0f, 1f);
                foreground = ApplyFade(foreground, normalizedLife);
                background = ApplyFade(background, normalizedLife);
            }

            var size = new Vector2(MathF.Max(1f, particle.Scale), MathF.Max(1f, particle.Scale));

            if (background.A > 0)
            {
                spriteBatch.DrawRectangle(particlePosition, size, background);
            }

            var tile = tileset.GetTile(particle.TileId);
            var uvX = (float)tile.SourceRect.Origin.X / tileset.Texture.Width;
            var uvY = (float)tile.SourceRect.Origin.Y / tileset.Texture.Height;
            var uvWidth = (float)tile.SourceRect.Size.X / tileset.Texture.Width;
            var uvHeight = (float)tile.SourceRect.Size.Y / tileset.Texture.Height;

            var sourceRect = new Rectangle<float>(uvX, uvY, uvWidth, uvHeight);

            spriteBatch.Draw(
                tileset.Texture,
                particlePosition,
                size,
                foreground,
                0f,
                Vector2.Zero,
                sourceRect,
                0f
            );
            rendered++;
        }
        _ = rendered;
    }

    private static LyColor ApplyFade(LyColor color, float factor)
    {
        var alpha = (byte)Math.Clamp(color.A * factor, 0f, 255f);

        return color.WithAlpha(alpha);
    }
}
