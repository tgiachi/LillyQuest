using LillyQuest.Core.Primitives;

namespace LillyQuest.RogueLike.Data.Internal;

public struct ColorData
{
    public ColorData(string name, LyColor color)
    {
        Name = name;
        Color = color;
    }

    public string Name { get; }
    public LyColor Color { get; }
}
