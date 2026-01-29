using System.Numerics;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Services;
using LillyQuest.Engine.Particles;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.Engine.Systems;

namespace LillyQuest.Engine.Services.Rendering;

public sealed class ParticlePixelRenderer : IParticlePixelRenderer
{
    private readonly ParticleSystem _particleSystem;

    public ParticlePixelRenderer(ParticleSystem particleSystem)
    {
        _particleSystem = particleSystem;
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

        var tileSize = new Vector2(tileWidth, tileHeight);
        var layerScale = screen.GetLayerRenderScale(layerIndex);
        var tileRenderScale = screen.TileRenderScale;

        foreach (var particle in _particleSystem.GetVisibleParticles())
        {
            var particlePosition = ComputeParticleScreenPosition(
                particle.Position,
                tileSize,
                tileRenderScale,
                layerScale,
                viewTileOffset,
                viewPixelOffset,
                layerPixelOffset
            );

            var color = particle.Color.A > 0 ? particle.Color : new LyColor(255, 255, 255, 255);

            if (particle.Flags.HasFlag(ParticleFlags.FadeOut))
            {
                var normalizedLife = particle.Lifetime / 2f;
                var alpha = (byte)Math.Clamp(255 * normalizedLife, 0, 255);
                color = color.WithAlpha(alpha);
            }

            var size = new Vector2(MathF.Max(1f, particle.Scale), MathF.Max(1f, particle.Scale));
            spriteBatch.DrawRectangle(particlePosition, size, color);
        }
    }

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
}
