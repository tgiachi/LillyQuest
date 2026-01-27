using LillyQuest.RogueLike.Data.Internal;
using LillyQuest.RogueLike.Interfaces;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Terrain;
using Serilog;

namespace LillyQuest.RogueLike.Services;

public class TerrainService : IDataLoaderReceiver
{
    private readonly ILogger _logger = Log.ForContext<TerrainService>();
    private readonly TileSetService _tileSetService;

    private readonly Dictionary<string, TerrainDefinitionJson> _terrainsById = new();
    private readonly Dictionary<string, ResolvedTerrainData> _resolvedById = new();

    public TerrainService(TileSetService tileSetService)
    {
        _tileSetService = tileSetService;
    }

    public Type[] GetLoadTypes()
        => [typeof(TerrainDefinitionJson)];

    public async Task LoadDataAsync(List<BaseJsonEntity> entities)
    {
        var terrains = entities.Cast<TerrainDefinitionJson>().ToList();

        foreach (var terrain in terrains)
        {
            if (string.IsNullOrEmpty(terrain.Id))
            {
                _logger.Warning("Terrain definition missing id");
                continue;
            }

            _terrainsById[terrain.Id] = terrain;

            EnsureDefaultTileset();

            if (!_tileSetService.TryGetTile(terrain.Id, out var tile))
            {
                _logger.Warning("Terrain {TerrainId} has no matching tile", terrain.Id);
                continue;
            }

            var resolved = new ResolvedTerrainData(
                terrain.Id,
                terrain.Name ?? string.Empty,
                terrain.Description ?? string.Empty,
                terrain.Flags ?? new List<string>(),
                terrain.MovementCost,
                terrain.Comment ?? string.Empty,
                terrain.Tags ?? new List<string>(),
                tile.Symbol,
                tile.FgColor,
                tile.BgColor,
                tile.Animation
            );

            _resolvedById[terrain.Id] = resolved;
        }
    }

    public void ClearData()
    {
        _terrainsById.Clear();
        _resolvedById.Clear();
    }

    public bool TryGetTerrain(string terrainId, out ResolvedTerrainData terrain)
    {
        terrain = null!;

        if (_resolvedById.TryGetValue(terrainId, out var resolved))
        {
            terrain = resolved;
            return true;
        }

        _logger.Warning("Terrain {TerrainId} not found or not resolved", terrainId);
        return false;
    }

    public bool VerifyLoadedData()
    {
        foreach (var terrain in _terrainsById.Values)
        {
            if (string.IsNullOrEmpty(terrain.Id))
            {
                throw new InvalidOperationException("Terrain with missing ID found during verification");
            }

            EnsureDefaultTileset();

            if (!_tileSetService.TryGetTile(terrain.Id, out _))
            {
                throw new InvalidOperationException($"Terrain {terrain.Id} has no matching tile during verification");
            }
        }

        return true;
    }

    private void EnsureDefaultTileset()
    {
        if (!string.IsNullOrEmpty(_tileSetService.DefaultTileset))
        {
            return;
        }

        if (_tileSetService.TryGetAnyTilesetName(out var tilesetName))
        {
            _tileSetService.DefaultTileset = tilesetName;
        }
    }
}
