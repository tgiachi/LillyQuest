using LillyQuest.Core.Primitives;

namespace LillyQuest.RogueLike.Maps.Tiles;

public class VisualTile
{
    public string Id { get; set; }

    public string Symbol { get; set; }

    public LyColor BackgroundColor { get; set; }

    public LyColor ForegroundColor { get; set; }

    public VisualTile() { }

    public VisualTile(string id, string symbol, LyColor backgroundColor, LyColor foregroundColor)
    {
        Id = id;
        Symbol = symbol;
        BackgroundColor = backgroundColor;
        ForegroundColor = foregroundColor;
    }

    public override string ToString()
        => $"{nameof(Id)}: {Id}, {nameof(Symbol)}: {Symbol}, {nameof(BackgroundColor)}: {BackgroundColor}, {nameof(ForegroundColor)}: {ForegroundColor}";
}
