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
    private FovSystem? _fovSystem;
    private LyQuestMap? _map;

    public GoRogueFOVProvider()
    {
    }

    public void SetFovSystem(FovSystem fovSystem)
    {
        _fovSystem = fovSystem;
    }

    public void SetMap(LyQuestMap map)
    {
        _map = map;
    }

    public bool IsVisible(int x, int y)
    {
        if (_map == null || _fovSystem == null)
        {
            return true; // No map/fov system = treat all as visible
        }
        
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
