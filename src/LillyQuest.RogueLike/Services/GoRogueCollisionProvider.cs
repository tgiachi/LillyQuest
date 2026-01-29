using System.Numerics;
using LillyQuest.Engine.Interfaces.Particles;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Maps;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.Services;

/// <summary>
/// Provides particle collision detection using GoRogue map data.
/// </summary>
public sealed class GoRogueCollisionProvider : IParticleCollisionProvider
{
    private readonly LyQuestMap _map;

    public GoRogueCollisionProvider(LyQuestMap map)
    {
        _map = map;
    }

    public bool IsBlocked(int x, int y)
    {
        var point = new Point(x, y);
        
        // Check if position is out of bounds
        if (x < 0 || y < 0 || x >= _map.Width || y >= _map.Height)
        {
            return true;
        }

        // Use GoRogue's WalkabilityView (already considers all layers and objects)
        return !_map.WalkabilityView[point];
    }

    public bool IsBlocked(Vector2 worldPosition)
    {
        var x = (int)worldPosition.X;
        var y = (int)worldPosition.Y;
        return IsBlocked(x, y);
    }
}
