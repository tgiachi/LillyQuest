using System.Numerics;

namespace LillyQuest.Engine.Interfaces.Features;

/// <summary>
/// Provides 2D transform data for an entity (position, rotation, scale, size).
/// </summary>
public interface ITransform2dFeature : IEntityFeature
{
    /// <summary>
    /// Gets or sets the world position.
    /// </summary>
    Vector2 Position { get; set; }

    /// <summary>
    /// Gets or sets the rotation in radians.
    /// </summary>
    float Rotation { get; set; }

    /// <summary>
    /// Gets or sets the local scale.
    /// </summary>
    Vector2 Scale { get; set; }

    /// <summary>
    /// Gets or sets the size of the entity in world units.
    /// </summary>
    Vector2 Size { get; set; }
}
