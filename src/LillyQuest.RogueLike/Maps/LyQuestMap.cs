using GoRogue.GameFramework;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.Maps;

/// <summary>
/// Game map that extends GoRogue's Map with LillyQuest-specific layers.
/// Rendering is handled separately by IMapRendererService.
/// </summary>
public class LyQuestMap : Map
{

    public string Name { get; set; }

    public int Level { get; set; }

    public LyQuestMap(int width, int height) : base(
        width,
        height,
        Enum.GetValues<MapLayer>().Length,
        Distance.Manhattan
    ) { }
}
