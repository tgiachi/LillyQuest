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
    private readonly List<Particle> _particles = new(1024);
    private readonly IParticleCollisionProvider _collisionProvider;
    private readonly IParticleFOVProvider _fovProvider;

    public uint Order => 145;
    public string Name => "ParticleSystem";
    public SystemQueryType QueryType => SystemQueryType.Updateable | SystemQueryType.Renderable;
    
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

    public void Update(double deltaTime)
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
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
            var newTileX = (int)newPosition.X;
            var newTileY = (int)newPosition.Y;
            
            // Check collision
            if (_collisionProvider.IsBlocked(newTileX, newTileY))
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

    public float GetParticleLifetime(int index)
    {
        return _particles[index].Lifetime;
    }

    public Vector2 GetParticlePosition(int index)
    {
        return _particles[index].Position;
    }

    public Vector2 GetParticleVelocity(int index)
    {
        return _particles[index].Velocity;
    }

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
    /// Emits a projectile particle that moves in a straight line.
    /// </summary>
    public void EmitProjectile(Vector2 from, Vector2 direction, float speed, int tileId, float lifetime = 5f)
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
            Scale = 1f,
            Color = default
        };
        
        Emit(particle);
    }

    /// <summary>
    /// Emits an explosion effect with particles radiating from a center point.
    /// </summary>
    public void EmitExplosion(Vector2 center, int tileId, int particleCount = 20, float speed = 100f, float lifetime = 0.5f)
    {
        for (int i = 0; i < particleCount; i++)
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
                Scale = 1f,
                Color = default
            };
            
            Emit(particle);
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
                // TODO: Radial decay
                break;
                
            case ParticleBehavior.Projectile:
                // No modification - constant velocity
                break;
        }
    }

    public void Initialize()
    {
    }

    public void ProcessEntities(GameTime gameTime, IGameEntityManager entityManager)
    {
        Update(gameTime.Elapsed.TotalSeconds);
    }
}
