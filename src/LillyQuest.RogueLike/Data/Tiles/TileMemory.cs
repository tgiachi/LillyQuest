using SadRogue.Primitives;

namespace LillyQuest.RogueLike.Data.Tiles;

/// <summary>
/// Stores visual information about a tile for fog of war display.
/// </summary>
public record TileMemory(char Symbol, Color ForegroundColor, Color BackgroundColor);
