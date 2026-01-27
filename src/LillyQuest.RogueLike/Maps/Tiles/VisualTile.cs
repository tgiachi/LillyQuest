using LillyQuest.Core.Primitives;

namespace LillyQuest.RogueLike.Maps.Tiles;

public class VisualTile
{
    public string Id { get; set; }

    public string Symbol { get; set; }

    public LyColor BackgroundColor { get; set; }

    public LyColor ForegroundColor { get; set; }
}
