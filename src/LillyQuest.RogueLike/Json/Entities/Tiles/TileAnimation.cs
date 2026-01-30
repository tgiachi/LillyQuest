using LillyQuest.RogueLike.Types;

namespace LillyQuest.RogueLike.Json.Entities.Tiles;

/// <summary>
/// Represents animation data for a tile, including the animation type, frames, and duration between frames.
/// </summary>
public class TileAnimation
{
    /// <summary>
    /// Gets or sets the animation type that determines how frames are played (Loop, PingPong, Random, or Once).
    /// </summary>
    public TileAnimationType Type { get; set; }

    /// <summary>
    /// Gets or sets the list of animation frames that make up this animation.
    /// </summary>
    public List<TileAnimationFrame> Frames { get; set; }

    /// <summary>
    /// Gets or sets the duration in milliseconds that each frame is displayed before moving to the next frame.
    /// </summary>
    public int FrameDurationMs { get; set; }
}
