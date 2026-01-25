using LillyQuest.RogueLike.Json.Entities.Base;

namespace LillyQuest.RogueLike.Json.Entities.Tiles;

/// <summary>
/// Represents the definition of a tile, including its visual representation, colors, and optional animation.
/// </summary>
public class TileDefinition : BaseJsonEntity
{
    /// <summary>
    /// Gets or sets the symbol (character) that represents this tile visually.
    /// </summary>
    public string Symbol { get; set;  }

    /// <summary>
    /// Gets or sets the foreground color of this tile.
    /// </summary>
    public string FgColor { get; set; }

    /// <summary>
    /// Gets or sets the background color of this tile.
    /// </summary>
    public string BgColor { get; set; }

    /// <summary>
    /// Gets or sets the animation for this tile, or null if the tile is not animated.
    /// </summary>
    public TileAnimation? Animation { get; set; }
}
