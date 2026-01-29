using LillyQuest.RogueLike.Data.Internal;
using LillyQuest.RogueLike.Interfaces;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Terrain;
using Serilog;

namespace LillyQuest.RogueLike.Services.Loaders;

public class TerrainService : IDataLoaderReceiver
{
    private readonly ILogger _logger = Log.ForContext<TerrainService>();
    private readonly TileSetService _tileSetService;

    private readonly Dictionary<string, TerrainDefinitionJson> _terrainsById = new();
    private readonly Dictionary<string, ResolvedTerrainData> _resolvedById = new();

    private readonly Dictionary<string, List<ResolvedTerrainData>> _resolvedByTag =
        new(StringComparer.OrdinalIgnoreCase);

    public TerrainService(TileSetService tileSetService)
        => _tileSetService = tileSetService;

    public void ClearData()
    {
        _terrainsById.Clear();
        _resolvedById.Clear();
        _resolvedByTag.Clear();
    }

    public Type[] GetLoadTypes()
        => [typeof(TerrainDefinitionJson)];

    public IReadOnlyList<ResolvedTerrainData> GetRandomTerrains(
        string categoryFilter,
        string subcategoryFilter,
        int count,
        Random? rng = null
    )
    {
        if (count <= 0)
        {
            return Array.Empty<ResolvedTerrainData>();
        }

        if (string.IsNullOrWhiteSpace(categoryFilter))
        {
            return [];
        }

        var candidates = _resolvedById.Values
                                      .Where(terrain => MatchesPattern(terrain.Category, categoryFilter));

        if (!string.IsNullOrWhiteSpace(subcategoryFilter))
        {
            candidates = candidates.Where(terrain => MatchesPattern(terrain.Subcategory, subcategoryFilter));
        }

        var list = candidates.ToList();

        if (list.Count <= count)
        {
            return list;
        }

        rng ??= Random.Shared;

        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list.Take(count).ToList();
    }

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
                terrain.Category ?? string.Empty,
                terrain.Subcategory ?? string.Empty,
                tile.Symbol,
                tile.FgColor,
                tile.BgColor,
                tile.Animation
            );

            _resolvedById[terrain.Id] = resolved;
            AddToTagIndex(resolved);
        }
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

    public bool TryGetTerrainByIdOrTag(
        string key,
        out ResolvedTerrainData terrain,
        out IReadOnlyList<ResolvedTerrainData> taggedTerrains
    )
    {
        terrain = null!;
        taggedTerrains = Array.Empty<ResolvedTerrainData>();

        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.Warning("Terrain id or tag is empty");

            return false;
        }

        if (_resolvedById.TryGetValue(key, out var resolved))
        {
            terrain = resolved;

            return true;
        }

        if (_resolvedByTag.TryGetValue(key, out var resolvedByTag) && resolvedByTag.Count > 0)
        {
            taggedTerrains = resolvedByTag;

            return true;
        }

        _logger.Warning("Terrain {TerrainKey} not found by id or tag", key);

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

    private void AddToTagIndex(ResolvedTerrainData resolved)
    {
        if (resolved.Tags is null || resolved.Tags.Count == 0)
        {
            return;
        }

        foreach (var tag in resolved.Tags)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                continue;
            }

            if (!_resolvedByTag.TryGetValue(tag, out var list))
            {
                list = new();
                _resolvedByTag[tag] = list;
            }

            list.Add(resolved);
        }
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

    private static bool MatchesPattern(string value, string pattern)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(pattern))
        {
            return false;
        }

        if (pattern == "*")
        {
            return true;
        }

        var startsWithWildcard = pattern.StartsWith("*", StringComparison.Ordinal);
        var endsWithWildcard = pattern.EndsWith("*", StringComparison.Ordinal);

        if (!startsWithWildcard && !endsWithWildcard)
        {
            return string.Equals(value, pattern, StringComparison.OrdinalIgnoreCase);
        }

        var trimmed = pattern.Trim('*');

        if (string.IsNullOrEmpty(trimmed))
        {
            return true;
        }

        if (startsWithWildcard && endsWithWildcard)
        {
            return value.Contains(trimmed, StringComparison.OrdinalIgnoreCase);
        }

        if (startsWithWildcard)
        {
            return value.EndsWith(trimmed, StringComparison.OrdinalIgnoreCase);
        }

        return value.StartsWith(trimmed, StringComparison.OrdinalIgnoreCase);
    }
}
