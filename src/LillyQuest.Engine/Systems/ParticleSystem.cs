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
                // Don't update position if collided and no Die flag
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

    public void Initialize()
    {
    }

    public void ProcessEntities(GameTime gameTime, IGameEntityManager entityManager)
    {
        Update(gameTime.Elapsed.TotalSeconds);
    }
}
