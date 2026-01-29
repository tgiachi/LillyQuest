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
            
            _particles[i] = particle;
        }
    }

    public float GetParticleLifetime(int index)
    {
        return _particles[index].Lifetime;
    }

    public void Initialize()
    {
    }

    public void ProcessEntities(GameTime gameTime, IGameEntityManager entityManager)
    {
        Update(gameTime.Elapsed.TotalSeconds);
    }
}
