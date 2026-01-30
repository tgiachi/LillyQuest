using System.Numerics;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Particles;
using LillyQuest.Engine.Interfaces.Systems;
using LillyQuest.Engine.Particles;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.Engine.Types;
using Silk.NET.Maths;

namespace LillyQuest.Engine.Systems;

/// <summary>
/// System that manages and updates particle effects.
/// </summary>
public sealed class ParticleSystem : ISystem
{
    private static readonly LyColor DefaultForeground = LyColor.White;
    private static readonly LyColor DefaultBackground = LyColor.Transparent;
    private const float DefaultScale = 6f;

    private readonly List<Particle> _particles = new(1024);
    private readonly IParticleCollisionProvider _collisionProvider;
    private readonly IParticleFOVProvider _fovProvider;

    public uint Order => 145;
    public string Name => "ParticleSystem";
    public SystemQueryType QueryType => SystemQueryType.Updateable;

    public int ParticleCount => _particles.Count;

    public ParticleSystem(
        IParticleCollisionProvider collisionProvider,
        IParticleFOVProvider fovProvider
    )
    {
        _collisionProvider = collisionProvider;
        _fovProvider = fovProvider;
    }

    public void Emit(Particle particle)
    {
        _particles.Add(particle);
    }

    /// <summary>
    /// Emits ambient particles (fire, smoke, etc.) with random variance.
    /// </summary>
    public void EmitAmbient(Vector2 position, int tileId, int count = 5, float lifetime = 2f)
        => EmitAmbient(position, tileId, count, lifetime, DefaultForeground, DefaultBackground, DefaultScale);

    public void EmitAmbient(
        Vector2 position,
        int tileId,
        int count,
        float lifetime,
        LyColor foregroundColor,
        LyColor backgroundColor,
        float scale
    )
    {
        var random = new Random();

        for (var i = 0; i < count; i++)
        {
            var particle = new Particle
            {
                Position = position +
                           new Vector2(
                               (float)(random.NextDouble() - 0.5) * 8f,
                               (float)(random.NextDouble() - 0.5) * 8f
                           ),
                Velocity = new Vector2(0, -20) +
                           new Vector2(
                               (float)(random.NextDouble() - 0.5) * 10f,
                               (float)(random.NextDouble() - 0.5) * 10f
                           ),
                Lifetime = lifetime,
                Behavior = ParticleBehavior.Ambient,
                TileId = tileId,
                Flags = ParticleFlags.FadeOut,
                Scale = scale,
                Color = default,
                ForegroundColor = foregroundColor,
                BackgroundColor = backgroundColor
            };

            Emit(particle);
        }
    }

    /// <summary>
    /// Emits an explosion effect with particles radiating from a center point.
    /// </summary>
    public void EmitExplosion(
        Vector2 center,
        int tileId,
        int particleCount = 20,
        float speed = 100f,
        float lifetime = 0.5f
    )
        => EmitExplosion(center, tileId, particleCount, speed, lifetime, DefaultForeground, DefaultBackground, DefaultScale);

    public void EmitExplosion(
        Vector2 center,
        int tileId,
        int particleCount,
        float speed,
        float lifetime,
        LyColor foregroundColor,
        LyColor backgroundColor,
        float scale
    )
    {
        for (var i = 0; i < particleCount; i++)
        {
            var angle = (float)(i * 2 * Math.PI / particleCount);
            var direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));

            var particle = new Particle
            {
                Position = center,
                Velocity = direction * speed,
                Lifetime = lifetime,
                Behavior = ParticleBehavior.Explosion,
                TileId = tileId,
                Flags = ParticleFlags.FadeOut | ParticleFlags.Die,
                Scale = scale,
                Color = default,
                ForegroundColor = foregroundColor,
                BackgroundColor = backgroundColor
            };

            Emit(particle);
        }
    }

    /// <summary>
    /// Emits a projectile particle that moves in a straight line.
    /// </summary>
    public void EmitProjectile(Vector2 from, Vector2 direction, float speed, int tileId, float lifetime = 5f)
        => EmitProjectile(from, direction, speed, tileId, lifetime, DefaultForeground, DefaultBackground, DefaultScale);

    public void EmitProjectile(
        Vector2 from,
        Vector2 direction,
        float speed,
        int tileId,
        float lifetime,
        LyColor foregroundColor,
        LyColor backgroundColor,
        float scale
    )
    {
        var normalizedDir = Vector2.Normalize(direction);

        var particle = new Particle
        {
            Position = from,
            Velocity = normalizedDir * speed,
            Lifetime = lifetime,
            Behavior = ParticleBehavior.Projectile,
            TileId = tileId,
            Flags = ParticleFlags.Die,
            Scale = scale,
            Color = default,
            ForegroundColor = foregroundColor,
            BackgroundColor = backgroundColor
        };

        Emit(particle);
    }

    public Particle GetParticle(int index)
        => _particles[index];

    public float GetParticleLifetime(int index)
        => _particles[index].Lifetime;

    public Vector2 GetParticlePosition(int index)
        => _particles[index].Position;

    public Vector2 GetParticleVelocity(int index)
        => _particles[index].Velocity;

    public List<Particle> GetVisibleParticles()
    {
        var visibleParticles = new List<Particle>();

        foreach (var particle in _particles)
        {
            var tileX = (int)particle.Position.X;
            var tileY = (int)particle.Position.Y;

            if (_fovProvider.IsVisible(tileX, tileY))
            {
                visibleParticles.Add(particle);
            }
        }

        return visibleParticles;
    }

    /// <summary>
    /// Renders all visible particles to the specified layer on the screen.
    /// </summary>
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

        foreach (var particle in GetVisibleParticles())
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
        }
    }

    private static Vector2 ComputeParticleScreenPosition(
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

    private static LyColor ApplyFade(LyColor color, float factor)
    {
        var alpha = (byte)Math.Clamp(color.A * factor, 0f, 255f);

        return color.WithAlpha(alpha);
    }

    public void Initialize() { }

    public void ProcessEntities(GameTime gameTime, IGameEntityManager entityManager)
    {
        // Update particles
        Update(gameTime.Elapsed.TotalSeconds);
    }

    public void Update(double deltaTime)
    {
        for (var i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            particle.Lifetime -= (float)deltaTime;

            if (particle.Lifetime <= 0)
            {
                _particles.RemoveAt(i);

                continue;
            }

            // Apply behavior modifications to velocity
            ApplyBehavior(ref particle, deltaTime);

            // Calculate new position
            var newPosition = particle.Position + particle.Velocity * (float)deltaTime;
            var oldTileX = (int)particle.Position.X;
            var oldTileY = (int)particle.Position.Y;
            var newTileX = (int)newPosition.X;
            var newTileY = (int)newPosition.Y;

            // Check collision only when crossing into a new tile
            if ((newTileX != oldTileX || newTileY != oldTileY) && _collisionProvider.IsBlocked(newTileX, newTileY))
            {
                // Handle collision based on flags
                if (particle.Flags.HasFlag(ParticleFlags.Die))
                {
                    _particles.RemoveAt(i);

                    continue;
                }

                if (particle.Flags.HasFlag(ParticleFlags.Bounce))
                {
                    // Reverse velocity with damping
                    particle.Velocity = -particle.Velocity * 0.5f;
                }

                // Don't update position if collided
            }
            else
            {
                // No collision, update position
                particle.Position = newPosition;
            }

            _particles[i] = particle;
        }
    }

    private void ApplyBehavior(ref Particle particle, double deltaTime)
    {
        switch (particle.Behavior)
        {
            case ParticleBehavior.Gravity:
                // Apply gravity acceleration (positive Y is down)
                particle.Velocity += new Vector2(0, 98f * (float)deltaTime);

                break;

            case ParticleBehavior.Ambient:
                // TODO: Random walk
                break;

            case ParticleBehavior.Explosion:
                // Slow down over time (radial decay)
                particle.Velocity *= 1f - (float)deltaTime * 2f;

                break;

            case ParticleBehavior.Projectile:
                // No modification - constant velocity
                break;
        }
    }
}
