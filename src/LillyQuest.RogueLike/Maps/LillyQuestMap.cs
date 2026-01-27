using GoRogue.GameFramework;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.Maps;

public class LillyQuestMap : Map
{
    public LillyQuestMap(int width, int height) : base(
        width,
        height,
        Enum.GetValues<MapLayer>().Length,
        Distance.Manhattan
    ) { }
}
