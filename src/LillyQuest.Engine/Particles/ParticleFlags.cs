namespace LillyQuest.Engine.Particles;

/// <summary>
/// Flags that modify particle behavior.
/// </summary>
[Flags]
public enum ParticleFlags
{
    /// <summary>
    /// No special behavior.
    /// </summary>
    None = 0,

    /// <summary>
    /// Bounce off walls on collision.
    /// </summary>
    Bounce = 1 << 0,

    /// <summary>
    /// Die immediately on collision.
    /// </summary>
    Die = 1 << 1,

    /// <summary>
    /// Fade out alpha over lifetime.
    /// </summary>
    FadeOut = 1 << 2,

    /// <summary>
    /// Apply gravity to velocity.
    /// </summary>
    Gravity = 1 << 3,
}
