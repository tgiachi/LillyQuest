using System.Numerics;
using System.Drawing;
using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Engine.Entities;

/// <summary>
/// Specialized game entity for UI screens with positioning, sizing, and visibility.
/// Provides automatic coordinate transformation and scissor rendering support.
/// </summary>
public class Screen : GameEntity
{
    /// <summary>
    /// World-space position (top-left corner).
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Screen size in pixels.
    /// </summary>
    public Vector2 Size { get; set; }

    /// <summary>
    /// Visibility flag. When false, screen is not rendered.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Bounding rectangle in world coordinates.
    /// </summary>
    public Rectangle Bounds => new Rectangle(
        (int)Position.X,
        (int)Position.Y,
        (int)Size.X,
        (int)Size.Y
    );

    /// <summary>
    /// Initializes a new Screen.
    /// </summary>
    public Screen(uint id, string name) : base(id, name)
    {
    }

    /// <summary>
    /// Transforms world coordinates to local screen coordinates (origin: top-left).
    /// </summary>
    public Vector2 WorldToLocal(Vector2 worldPos) => worldPos - Position;

    /// <summary>
    /// Transforms local screen coordinates to world coordinates.
    /// </summary>
    public Vector2 LocalToWorld(Vector2 localPos) => localPos + Position;

    /// <summary>
    /// Tests if a world-space point is within the screen bounds.
    /// </summary>
    public bool ContainsPoint(Vector2 worldPos)
    {
        return worldPos.X >= Position.X &&
               worldPos.X <= Position.X + Size.X &&
               worldPos.Y >= Position.Y &&
               worldPos.Y <= Position.Y + Size.Y;
    }
}
