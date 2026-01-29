using System.Numerics;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Engine.Particles;

/// <summary>
/// Represents a single particle in the particle system.
/// Lightweight value type with public fields for performance.
/// </summary>
#pragma warning disable CA1051 // Do not declare visible instance fields - struct for performance
public struct Particle
{
    /// <summary>
    /// World position of the particle (sub-tile precision).
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// Velocity in pixels/second.
    /// </summary>
    public Vector2 Velocity;

    /// <summary>
    /// Remaining lifetime in seconds.
    /// </summary>
    public float Lifetime;

    /// <summary>
    /// Behavior type that defines how the particle moves.
    /// </summary>
    public ParticleBehavior Behavior;

    /// <summary>
    /// Tile ID to render.
    /// </summary>
    public int TileId;

    /// <summary>
    /// Tint color for rendering.
    /// </summary>
    public LyColor Color;

    /// <summary>
    /// Scale factor (1.0 = normal tile size).
    /// </summary>
    public float Scale;
}
