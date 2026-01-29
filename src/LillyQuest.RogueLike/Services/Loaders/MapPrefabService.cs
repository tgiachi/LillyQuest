using LillyQuest.RogueLike.Interfaces;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Prefabs;
using LillyQuest.RogueLike.Utils;

namespace LillyQuest.RogueLike.Services.Loaders;

/// <summary>
/// Service for managing map prefab definitions.
/// </summary>
public sealed class MapPrefabService : IDataLoaderReceiver
{
    private readonly TerrainService _terrainService;
    private readonly ItemService _itemService;
    private readonly CreatureService _creatureService;
    private readonly Dictionary<string, MapPrefabDefinitionJson> _prefabsById = new();

    public MapPrefabService(
        TerrainService terrainService,
        ItemService itemService,
        CreatureService creatureService
    )
    {
        _terrainService = terrainService;
        _itemService = itemService;
        _creatureService = creatureService;
    }

    public void ClearData()
    {
        _prefabsById.Clear();
    }

    public Type[] GetLoadTypes()
        => [typeof(MapPrefabDefinitionJson)];

    public MapPrefabDefinitionJson? GetRandomPrefab(string categoryFilter, Random? rng = null)
        => GetRandomPrefab(categoryFilter, null, rng);

    public MapPrefabDefinitionJson? GetRandomPrefab(
        string categoryFilter,
        string? subcategoryFilter,
        Random? rng = null
    )
    {
        if (string.IsNullOrWhiteSpace(categoryFilter))
        {
            return null;
        }

        var candidates = _prefabsById.Values
                                     .Where(prefab => PatternMatchUtils.Matches(prefab.Category, categoryFilter));

        if (!string.IsNullOrWhiteSpace(subcategoryFilter))
        {
            candidates = candidates.Where(prefab => PatternMatchUtils.Matches(prefab.Subcategory, subcategoryFilter));
        }

        var list = candidates.ToList();

        if (list.Count == 0)
        {
            return null;
        }

        rng ??= Random.Shared;

        return list[rng.Next(list.Count)];
    }

    public Task LoadDataAsync(List<BaseJsonEntity> entities)
    {
        foreach (var entity in entities.Cast<MapPrefabDefinitionJson>())
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                continue;
            }

            _prefabsById[entity.Id] = entity;
        }

        return Task.CompletedTask;
    }

    public bool TryGetPrefab(string prefabId, out MapPrefabDefinitionJson prefab)
    {
        prefab = null!;

        if (_prefabsById.TryGetValue(prefabId, out var resolved))
        {
            prefab = resolved;

            return true;
        }

        return false;
    }

    public bool VerifyLoadedData()
    {
        foreach (var prefab in _prefabsById.Values)
        {
            if (prefab.Content is null || prefab.Content.Count == 0)
            {
                throw new InvalidOperationException($"Map prefab {prefab.Id} has empty content");
            }

            if (prefab.Palette is null)
            {
                throw new InvalidOperationException($"Map prefab {prefab.Id} has missing palette");
            }

            // Validate palette references
            foreach (var (symbol, entry) in prefab.Palette)
            {
                if (!string.IsNullOrWhiteSpace(entry.Terrain))
                {
                    // Skip validation for pattern-based references (category/subcategory or wildcard)
                    if (!entry.Terrain.Contains('/') && !entry.Terrain.Contains('*'))
                    {
                        if (!_terrainService.TryGetTerrain(entry.Terrain, out _))
                        {
                            throw new InvalidOperationException(
                                $"Map prefab {prefab.Id} references missing terrain '{entry.Terrain}' in palette symbol '{symbol}'"
                            );
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(entry.Creature))
                {
                    // Skip validation for pattern-based references
                    if (!entry.Creature.Contains('/') && !entry.Creature.Contains('*'))
                    {
                        if (!_creatureService.TryGetCreature(entry.Creature, out _))
                        {
                            throw new InvalidOperationException(
                                $"Map prefab {prefab.Id} references missing creature '{entry.Creature}' in palette symbol '{symbol}'"
                            );
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(entry.Item))
                {
                    // Skip validation for pattern-based references
                    if (!entry.Item.Contains('/') && !entry.Item.Contains('*'))
                    {
                        if (!_itemService.TryGetItem(entry.Item, out _))
                        {
                            throw new InvalidOperationException(
                                $"Map prefab {prefab.Id} references missing item '{entry.Item}' in palette symbol '{symbol}'"
                            );
                        }
                    }
                }
            }
        }

        return true;
    }
}
