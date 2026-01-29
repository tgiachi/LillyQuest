using System.Numerics;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Particles;
using LillyQuest.Engine.Interfaces.Systems;
using LillyQuest.Engine.Particles;
using LillyQuest.Engine.Types;

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
        IParticleFOVProvider fovProvider)
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
