using System.Numerics;

namespace LillyQuest.Engine.Interfaces.Particles;

/// <summary>
/// Provides field of view (FOV) information for particles.
/// </summary>
public interface IParticleFOVProvider
{
    /// <summary>
    /// Checks if a tile position is visible in the current FOV.
    /// </summary>
    bool IsVisible(int x, int y);

    /// <summary>
    /// Checks if a world position is visible in the current FOV.
    /// </summary>
    bool IsVisible(Vector2 worldPosition);
}
