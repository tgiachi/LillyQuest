using System.Numerics;

namespace LillyQuest.RogueLike.Data.Configs;

public class WorldManagerConfig
{
    public Vector2 OverworldSize { get; set; }

    public Vector2 MapSize { get; set; }

    public WorldManagerConfig()
    {
        OverworldSize = new Vector2(100, 100);
        MapSize = new Vector2(100, 100);
    }
}
