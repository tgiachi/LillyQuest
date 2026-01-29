using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Json.Entities.Tiles;

namespace LillyQuest.RogueLike.Data.Internal;

public record ResolvedTerrainData(
    string Id,
    string Name,
    string Description,
    List<string> Flags,
    int MovementCost,
    string Comment,
    List<string> Tags,
    string Category,
    string Subcategory,
    string TileSymbol,
    LyColor TileFgColor,
    LyColor TileBgColor,
    TileAnimation? TileAnimation
);
