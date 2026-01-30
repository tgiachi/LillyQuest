using LillyQuest.RogueLike.Interfaces;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Items;
using LillyQuest.RogueLike.Utils;

namespace LillyQuest.RogueLike.Services.Loaders;

/// <summary>
/// Service for managing item definitions.
/// </summary>
public class ItemService : IDataLoaderReceiver
{
    private readonly LootTableService _lootTableService;
    private readonly Dictionary<string, ItemDefinitionJson> _itemsById = new();

    public ItemService(LootTableService lootTableService)
        => _lootTableService = lootTableService;

    public void ClearData()
    {
        _itemsById.Clear();
    }

    public Type[] GetLoadTypes()
        => [typeof(ItemDefinitionJson)];

    public ItemDefinitionJson? GetRandomItem(string categoryFilter, Random? rng = null)
        => GetRandomItem(categoryFilter, null, rng);

    public ItemDefinitionJson? GetRandomItem(
        string categoryFilter,
        string? subcategoryFilter,
        Random? rng = null
    )
    {
        if (string.IsNullOrWhiteSpace(categoryFilter))
        {
            return null;
        }

        var candidates = _itemsById.Values
                                   .Where(item => PatternMatchUtils.Matches(item.Category, categoryFilter));

        if (!string.IsNullOrWhiteSpace(subcategoryFilter))
        {
            candidates = candidates.Where(item => PatternMatchUtils.Matches(item.Subcategory, subcategoryFilter));
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
        foreach (var entity in entities.Cast<ItemDefinitionJson>())
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                continue;
            }

            _itemsById[entity.Id] = entity;
        }

        return Task.CompletedTask;
    }

    public bool TryGetItem(string itemId, out ItemDefinitionJson item)
    {
        item = null!;

        if (_itemsById.TryGetValue(itemId, out var resolved))
        {
            item = resolved;

            return true;
        }

        return false;
    }

    public bool VerifyLoadedData()
    {
        foreach (var item in _itemsById.Values)
        {
            if (string.IsNullOrWhiteSpace(item.Id))
            {
                throw new InvalidOperationException("Item with missing ID found during verification");
            }

            if (item.Slot.HasValue && !Enum.IsDefined(item.Slot.Value))
            {
                throw new InvalidOperationException($"Item {item.Id} has invalid equipment slot value");
            }

            if (item.IsContainer && !item.Capacity.HasValue)
            {
                throw new InvalidOperationException($"Item {item.Id} is a container but has no capacity defined");
            }

            if (!string.IsNullOrWhiteSpace(item.LootTable))
            {
                if (!_lootTableService.TryGetLootTable(item.LootTable, out _))
                {
                    throw new InvalidOperationException($"Item {item.Id} references missing loot table '{item.LootTable}'");
                }
            }
        }

        return true;
    }
}
