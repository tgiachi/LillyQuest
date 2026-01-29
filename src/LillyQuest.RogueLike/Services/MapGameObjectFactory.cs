using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Services.Loaders;
using Serilog;

namespace LillyQuest.RogueLike.Services;

public class MapGameObjectFactory : IMapGameObjectFactory
{
    private readonly ILogger _logger = Log.ForContext<MapGameObjectFactory>();

    private readonly TileSetService _tileSetService;

    private readonly TerrainService _terrainService;

    public MapGameObjectFactory(TileSetService tileSetService, TerrainService terrainService)
    {
        _tileSetService = tileSetService;
        _terrainService = terrainService;
    }

    public CreatureGameObject CreateCreature(string creatureIdOrTag, int x, int y)
    {
        return null;
    }

    public TerrainGameObject CreateTerrain(string terrainIdOrTag, int x, int y)
    {
        return null;
    }
}
