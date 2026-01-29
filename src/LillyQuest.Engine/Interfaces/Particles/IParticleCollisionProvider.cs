using System.Numerics;

namespace LillyQuest.Engine.Interfaces.Particles;

/// <summary>
/// Provides collision detection for particles.
/// </summary>
public interface IParticleCollisionProvider
{
    /// <summary>
    /// Checks if a tile position is blocked (e.g., wall, obstacle).
    /// </summary>
    bool IsBlocked(int x, int y);

    /// <summary>
    /// Checks if a world position is blocked.
    /// </summary>
    bool IsBlocked(Vector2 worldPosition);
}
