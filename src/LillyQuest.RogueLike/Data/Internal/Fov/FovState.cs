using GoRogue.FOV;
using LillyQuest.RogueLike.Data.Tiles;
using LillyQuest.RogueLike.Maps;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.Data.Internal.Fov;

internal sealed class FovState
{
    public LyQuestMap Map { get; }
    public RecursiveShadowcastingFOV Fov { get; }
    public HashSet<Point> CurrentVisibleTiles { get; } = new();
    public HashSet<Point> ExploredTiles { get; } = new();
    public Dictionary<Point, TileMemory> TileMemory { get; } = new();
    public Dictionary<Point, float> VisibilityFalloff { get; } = new();
    public Point LastViewerPosition { get; set; } = new(-1, -1);

    public FovState(LyQuestMap map, RecursiveShadowcastingFOV fov)
    {
        Map = map;
        Fov = fov;
    }
}
