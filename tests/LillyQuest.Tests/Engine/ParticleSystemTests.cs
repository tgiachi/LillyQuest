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

    [Test]
    public void Update_MovesParticlesByVelocity()
    {
        // Arrange
        var collisionProvider = new FakeCollisionProvider();
        var fovProvider = new FakeFOVProvider();
        var system = new ParticleSystem(collisionProvider, fovProvider);
        
        var particle = new Particle
        {
            Position = new Vector2(10, 20),
            Velocity = new Vector2(5, -10), // 5 pixels/sec right, 10 pixels/sec up
            Lifetime = 10f
        };
        
        system.Emit(particle);

        // Act - advance 0.5 seconds
        system.Update(0.5);

        // Assert - position should be updated by velocity * deltaTime
        var newPos = system.GetParticlePosition(0);
        Assert.That(newPos.X, Is.EqualTo(12.5f).Within(0.001f));  // 10 + 5*0.5
        Assert.That(newPos.Y, Is.EqualTo(15.0f).Within(0.001f));  // 20 + (-10)*0.5
    }

    [Test]
    public void Update_ParticleWithDieFlag_DiesOnCollision()
    {
        // Arrange
        var collisionProvider = new FakeCollisionProvider();
        collisionProvider.SetBlocked(11, 20, isBlocked: true); // Wall at destination
        
        var fovProvider = new FakeFOVProvider();
        var system = new ParticleSystem(collisionProvider, fovProvider);
        
        var particle = new Particle
        {
            Position = new Vector2(10, 20),
            Velocity = new Vector2(10, 0), // Moving right, will reach x=11 in 0.1 sec
            Lifetime = 10f,
            Flags = ParticleFlags.Die
        };
        
        system.Emit(particle);

        // Act - advance time to move particle into wall
        system.Update(0.1); // Will move to x=11 which is blocked

        // Assert - particle should be removed
        Assert.That(system.ParticleCount, Is.EqualTo(0));
    }

    [Test]
    public void Update_ParticleWithoutDieFlag_StopsAtCollision()
    {
        // Arrange
        var collisionProvider = new FakeCollisionProvider();
        collisionProvider.SetBlocked(11, 20, isBlocked: true);
        
        var fovProvider = new FakeFOVProvider();
        var system = new ParticleSystem(collisionProvider, fovProvider);
        
        var particle = new Particle
        {
            Position = new Vector2(10, 20),
            Velocity = new Vector2(10, 0), // Moving right towards wall
            Lifetime = 10f,
            Flags = ParticleFlags.None
        };
        
        system.Emit(particle);

        // Act
        system.Update(0.1);

        // Assert - particle should still exist at original position
        Assert.That(system.ParticleCount, Is.EqualTo(1));
        var pos = system.GetParticlePosition(0);
        Assert.That(pos.X, Is.EqualTo(10f).Within(0.001f)); // Didn't move
        Assert.That(pos.Y, Is.EqualTo(20f).Within(0.001f));
    }

    [Test]
    public void Update_ParticleWithBounceFlag_ReflectsVelocityOnCollision()
    {
        // Arrange
        var collisionProvider = new FakeCollisionProvider();
        collisionProvider.SetBlocked(11, 20, isBlocked: true);
        
        var fovProvider = new FakeFOVProvider();
        var system = new ParticleSystem(collisionProvider, fovProvider);
        
        var particle = new Particle
        {
            Position = new Vector2(10, 20),
            Velocity = new Vector2(10, 0), // Moving right
            Lifetime = 10f,
            Flags = ParticleFlags.Bounce
        };
        
        system.Emit(particle);

        // Act
        system.Update(0.1);

        // Assert - particle should bounce back (velocity reversed with damping)
        Assert.That(system.ParticleCount, Is.EqualTo(1));
        var velocity = system.GetParticleVelocity(0);
        Assert.That(velocity.X, Is.LessThan(0)); // Reversed direction
        Assert.That(MathF.Abs(velocity.X), Is.LessThan(10f)); // Damped (less than original)
    }

    [Test]
    public void Update_GravityBehavior_AcceleratesDownward()
    {
        // Arrange
        var collisionProvider = new FakeCollisionProvider();
        var fovProvider = new FakeFOVProvider();
        var system = new ParticleSystem(collisionProvider, fovProvider);
        
        var particle = new Particle
        {
            Position = new Vector2(10, 10),
            Velocity = new Vector2(0, 0), // Starting stationary
            Lifetime = 10f,
            Behavior = ParticleBehavior.Gravity
        };
        
        system.Emit(particle);

        // Act - update twice to see acceleration
        system.Update(0.1);
        var velocityAfterFirst = system.GetParticleVelocity(0);
        
        system.Update(0.1);
        var velocityAfterSecond = system.GetParticleVelocity(0);

        // Assert - velocity.Y should increase (falling down, positive Y)
        Assert.That(velocityAfterFirst.Y, Is.GreaterThan(0));
        Assert.That(velocityAfterSecond.Y, Is.GreaterThan(velocityAfterFirst.Y));
    }

    [Test]
    public void GetVisibleParticles_ReturnsOnlyParticlesInFOV()
    {
        // Arrange
        var collisionProvider = new FakeCollisionProvider();
        var fovProvider = new FakeFOVProvider();
        var system = new ParticleSystem(collisionProvider, fovProvider);
        
        // Particle in FOV
        system.Emit(new Particle { Position = new Vector2(5, 5), Lifetime = 1f });
        fovProvider.SetVisible(5, 5, isVisible: true);
        
        // Particle outside FOV
        system.Emit(new Particle { Position = new Vector2(10, 10), Lifetime = 1f });
        fovProvider.SetVisible(10, 10, isVisible: false);
        
        // Another particle in FOV
        system.Emit(new Particle { Position = new Vector2(6, 6), Lifetime = 1f });
        fovProvider.SetVisible(6, 6, isVisible: true);

        // Act
        var visibleParticles = system.GetVisibleParticles();

        // Assert - should return only the 2 visible particles
        Assert.That(visibleParticles.Count, Is.EqualTo(2));
    }

    [Test]
    public void EmitProjectile_CreatesParticleWithCorrectProperties()
    {
        // Arrange
        var collisionProvider = new FakeCollisionProvider();
        var fovProvider = new FakeFOVProvider();
        var system = new ParticleSystem(collisionProvider, fovProvider);
        
        var from = new Vector2(10, 20);
        var direction = new Vector2(1, 0); // Right
        var speed = 100f;
        var tileId = 42;

        // Act
        system.EmitProjectile(from, direction, speed, tileId);

        // Assert
        Assert.That(system.ParticleCount, Is.EqualTo(1));
        var pos = system.GetParticlePosition(0);
        var vel = system.GetParticleVelocity(0);
        
        Assert.That(pos, Is.EqualTo(from));
        Assert.That(vel.X, Is.EqualTo(100f).Within(0.001f)); // Normalized direction * speed
        Assert.That(vel.Y, Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void EmitExplosion_CreatesMultipleParticlesRadially()
    {
        // Arrange
        var collisionProvider = new FakeCollisionProvider();
        var fovProvider = new FakeFOVProvider();
        var system = new ParticleSystem(collisionProvider, fovProvider);
        
        var center = new Vector2(50, 50);
        var tileId = 99;
        var particleCount = 8;

        // Act
        system.EmitExplosion(center, tileId, particleCount);

        // Assert
        Assert.That(system.ParticleCount, Is.EqualTo(particleCount));
        
        // Check that particles are moving away from center in different directions
        var velocities = new List<Vector2>();
        for (int i = 0; i < particleCount; i++)
        {
            velocities.Add(system.GetParticleVelocity(i));
        }
        
        // All velocities should be different (radial pattern)
        Assert.That(velocities.Distinct().Count(), Is.EqualTo(particleCount));
    }

    private sealed class FakeCollisionProvider : IParticleCollisionProvider
    {
        private readonly Dictionary<(int X, int Y), bool> _blockedTiles = new();

        public void SetBlocked(int x, int y, bool isBlocked)
        {
            _blockedTiles[(x, y)] = isBlocked;
        }

        public bool IsBlocked(int x, int y)
        {
            return _blockedTiles.TryGetValue((x, y), out var blocked) && blocked;
        }

        public bool IsBlocked(Vector2 worldPosition)
        {
            var x = (int)worldPosition.X;
            var y = (int)worldPosition.Y;
            return IsBlocked(x, y);
        }
    }

    private sealed class FakeFOVProvider : IParticleFOVProvider
    {
        private readonly Dictionary<(int X, int Y), bool> _visibleTiles = new();

        public void SetVisible(int x, int y, bool isVisible)
        {
            _visibleTiles[(x, y)] = isVisible;
        }

        public bool IsVisible(int x, int y)
        {
            return _visibleTiles.TryGetValue((x, y), out var visible) && visible;
        }

        public bool IsVisible(Vector2 worldPosition)
        {
            var x = (int)worldPosition.X;
            var y = (int)worldPosition.Y;
            return IsVisible(x, y);
        }
    }
}
