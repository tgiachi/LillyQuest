namespace LillyQuest.RogueLike.Json.Entities.Tiles;

/// <summary>
/// Represents a single frame in a tile animation, containing the symbol and optional colors to display.
/// </summary>
public class TileAnimationFrame
{
    /// <summary>
    /// Gets or sets the symbol (character) to display for this animation frame.
    /// </summary>
    public string Symbol { get; set; }

    /// <summary>
    /// Gets or sets the foreground color for this frame, or null to use the default color.
    /// </summary>
    public string? FgColor { get; set; }

    /// <summary>
    /// Gets or sets the background color for this frame, or null to use the default color.
    /// </summary>
    public string? BgColor { get; set; }
}
