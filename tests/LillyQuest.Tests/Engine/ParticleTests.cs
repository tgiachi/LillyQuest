using System.Numerics;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Particles;

namespace LillyQuest.Tests.Engine;

public class ParticleTests
{
    [Test]
    public void Particle_CanBeInitializedWithValues()
    {
        // Arrange
        var position = new Vector2(10, 20);
        var velocity = new Vector2(5, -3);
        var lifetime = 2.5f;

        // Act
        var particle = new Particle
        {
            Position = position,
            Velocity = velocity,
            Lifetime = lifetime,
            TileId = 42,
            Color = LyColor.Red,
            Scale = 1.5f
        };

        // Assert
        Assert.That(particle.Position, Is.EqualTo(position));
        Assert.That(particle.Velocity, Is.EqualTo(velocity));
        Assert.That(particle.Lifetime, Is.EqualTo(lifetime));
        Assert.That(particle.TileId, Is.EqualTo(42));
        Assert.That(particle.Color, Is.EqualTo(LyColor.Red));
        Assert.That(particle.Scale, Is.EqualTo(1.5f));
    }

    [Test]
    public void Particle_CanSetBehavior()
    {
        // Arrange & Act
        var particle = new Particle
        {
            Behavior = ParticleBehavior.Projectile
        };

        // Assert
        Assert.That(particle.Behavior, Is.EqualTo(ParticleBehavior.Projectile));
    }

    [Test]
    public void Particle_CanSetFlags()
    {
        // Arrange & Act
        var particle = new Particle
        {
            Flags = ParticleFlags.Die | ParticleFlags.FadeOut
        };

        // Assert
        Assert.That(particle.Flags.HasFlag(ParticleFlags.Die), Is.True);
        Assert.That(particle.Flags.HasFlag(ParticleFlags.FadeOut), Is.True);
    }

    [Test]
    public void Particle_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var particle = new Particle();

        // Assert
        Assert.That(particle.Position, Is.EqualTo(Vector2.Zero));
        Assert.That(particle.Velocity, Is.EqualTo(Vector2.Zero));
        Assert.That(particle.Lifetime, Is.EqualTo(0f));
    }

    [Test]
    public void ParticleBehavior_HasExpectedValues()
    {
        // Assert - verify enum values exist
        Assert.That(ParticleBehavior.Projectile, Is.EqualTo(ParticleBehavior.Projectile));
        Assert.That(ParticleBehavior.Ambient, Is.EqualTo(ParticleBehavior.Ambient));
        Assert.That(ParticleBehavior.Explosion, Is.EqualTo(ParticleBehavior.Explosion));
        Assert.That(ParticleBehavior.Gravity, Is.EqualTo(ParticleBehavior.Gravity));
    }

    [Test]
    public void ParticleFlags_CanBeCombined()
    {
        // Arrange & Act
        var combined = ParticleFlags.Bounce | ParticleFlags.FadeOut;

        // Assert
        Assert.That(combined.HasFlag(ParticleFlags.Bounce), Is.True);
        Assert.That(combined.HasFlag(ParticleFlags.FadeOut), Is.True);
        Assert.That(combined.HasFlag(ParticleFlags.Die), Is.False);
    }

    [Test]
    public void ParticleFlags_HasExpectedValues()
    {
        // Assert - verify enum values exist
        Assert.That(ParticleFlags.None, Is.EqualTo(ParticleFlags.None));
        Assert.That(ParticleFlags.Bounce, Is.EqualTo(ParticleFlags.Bounce));
        Assert.That(ParticleFlags.Die, Is.EqualTo(ParticleFlags.Die));
        Assert.That(ParticleFlags.FadeOut, Is.EqualTo(ParticleFlags.FadeOut));
        Assert.That(ParticleFlags.Gravity, Is.EqualTo(ParticleFlags.Gravity));
    }
}
