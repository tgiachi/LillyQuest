using LillyQuest.Core.Primitives;
using LillyQuest.Core.Types;

namespace LillyQuest.RogueLike.Maps.Tiles;

public class VisualTile
{
    public string Id { get; set; }
    public string Symbol { get; set; }

    public LyColor? BackgroundColor { get; set; }

    public LyColor ForegroundColor { get; set; }

    public TileFlipType Flip { get; set; }

    public VisualTile() { }

    public VisualTile(
        string id,
        string symbol,
        LyColor foregroundColor,
        LyColor? backgroundColor = null,
        TileFlipType flip = TileFlipType.None
    )
    {
        Id = id;
        Symbol = symbol;
        BackgroundColor = backgroundColor;
        ForegroundColor = foregroundColor;
        Flip = flip;
    }

    public override string ToString()
        => $"{nameof(Id)}: {Id}, {nameof(Symbol)}: {Symbol}, {nameof(BackgroundColor)}: {BackgroundColor}, {nameof(ForegroundColor)}: {ForegroundColor} , {nameof(Flip)}: {Flip}";
}
