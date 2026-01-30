namespace LillyQuest.Engine.Particles;

/// <summary>
/// Defines how a particle behaves and moves.
/// </summary>
public enum ParticleBehavior
{
    /// <summary>
    /// Linear movement, stops on collision.
    /// </summary>
    Projectile,

    /// <summary>
    /// Random walk with brownian motion, bounces on collision.
    /// </summary>
    Ambient,

    /// <summary>
    /// Radial expansion from origin, slows down over time.
    /// </summary>
    Explosion,

    /// <summary>
    /// Affected by gravity (falling).
    /// </summary>
    Gravity
}
