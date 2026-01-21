namespace LillyQuest.Core.Types;

[Flags]
public enum TileFlipType
{
    None = 0,
    FlipHorizontal = 1 << 0,
    FlipVertical = 1 << 1
}
