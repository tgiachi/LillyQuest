using System.Numerics;
using LillyQuest.Engine.Interfaces.Particles;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Systems;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.Services;

/// <summary>
/// Provides particle FOV checks using GoRogue's FOV system.
/// </summary>
public sealed class GoRogueFOVProvider : IParticleFOVProvider
{
    private readonly FovSystem _fovSystem;
    private readonly LyQuestMap _map;

    public GoRogueFOVProvider(FovSystem fovSystem, LyQuestMap map)
    {
        _fovSystem = fovSystem;
        _map = map;
    }

    public bool IsVisible(int x, int y)
    {
        var point = new Point(x, y);
        return _fovSystem.IsVisible(_map, point);
    }

    public bool IsVisible(Vector2 worldPosition)
    {
        var x = (int)worldPosition.X;
        var y = (int)worldPosition.Y;
        return IsVisible(x, y);
    }
}
