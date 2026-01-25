namespace LillyQuest.RogueLike.Types;

/// <summary>
/// Specifies how animation frames are played in a tile animation.
/// </summary>
public enum TileAnimationType : byte
{
    /// <summary>
    /// Loops through frames continuously: 0→1→2→3→0...
    /// </summary>
    Loop,

    /// <summary>
    /// Plays frames forward then backward: 0→1→2→3→2→1→0...
    /// </summary>
    PingPong,

    /// <summary>
    /// Displays a random frame on each animation update.
    /// </summary>
    Random,

    /// <summary>
    /// Plays the animation once and stops on the last frame.
    /// </summary>
    Once
}
