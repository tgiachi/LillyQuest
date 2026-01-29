using System.Numerics;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Particles;
using LillyQuest.Engine.Particles;
using LillyQuest.Engine.Systems;

namespace LillyQuest.Tests.Engine;

public class ParticleSystemTests
{
    [Test]
    public void Emit_AddsParticleToSystem()
    {
        // Arrange
        var collisionProvider = new FakeCollisionProvider();
        var fovProvider = new FakeFOVProvider();
        var system = new ParticleSystem(collisionProvider, fovProvider);

        var particle = new Particle
        {
            Position = new Vector2(10, 20),
            Lifetime = 1.0f
        };

        // Act
        system.Emit(particle);

        // Assert
        Assert.That(system.ParticleCount, Is.EqualTo(1));
    }

    [Test]
    public void Emit_CanAddMultipleParticles()
    {
        // Arrange
        var collisionProvider = new FakeCollisionProvider();
        var fovProvider = new FakeFOVProvider();
        var system = new ParticleSystem(collisionProvider, fovProvider);

        // Act
        system.Emit(new Particle { Lifetime = 1f });
        system.Emit(new Particle { Lifetime = 1f });
        system.Emit(new Particle { Lifetime = 1f });

        // Assert
        Assert.That(system.ParticleCount, Is.EqualTo(3));
    }

    [Test]
    public void Update_ReducesParticleLifetime()
    {
        // Arrange
        var collisionProvider = new FakeCollisionProvider();
        var fovProvider = new FakeFOVProvider();
        var system = new ParticleSystem(collisionProvider, fovProvider);
        
        var particle = new Particle
        {
            Position = Vector2.Zero,
            Lifetime = 2.0f
        };
        
        system.Emit(particle);

        // Act
        system.Update(0.5);

        // Assert
        var remainingLifetime = system.GetParticleLifetime(0);
        Assert.That(remainingLifetime, Is.EqualTo(1.5f).Within(0.001f));
    }

    [Test]
    public void Update_RemovesDeadParticles()
    {
        // Arrange
        var collisionProvider = new FakeCollisionProvider();
        var fovProvider = new FakeFOVProvider();
        var system = new ParticleSystem(collisionProvider, fovProvider);
        
        system.Emit(new Particle { Lifetime = 0.5f });
        system.Emit(new Particle { Lifetime = 2.0f });
        system.Emit(new Particle { Lifetime = 0.3f });

        // Act - advance time beyond short lifetimes
        system.Update(1.0);

        // Assert - only particle with 2.0f lifetime should remain
        Assert.That(system.ParticleCount, Is.EqualTo(1));
        Assert.That(system.GetParticleLifetime(0), Is.EqualTo(1.0f).Within(0.001f));
    }

    private sealed class FakeCollisionProvider : IParticleCollisionProvider
    {
        public bool IsBlocked(int x, int y) => false;
        public bool IsBlocked(Vector2 worldPosition) => false;
    }

    private sealed class FakeFOVProvider : IParticleFOVProvider
    {
        public bool IsVisible(int x, int y) => true;
        public bool IsVisible(Vector2 worldPosition) => true;
    }
}
