using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Json.Entities.Tiles;

namespace LillyQuest.RogueLike.Data.Internal;

public record ResolvedTileData(
    string Id,
    string Symbol,
    LyColor FgColor,
    LyColor BgColor,
    TileAnimation? Animation
);
