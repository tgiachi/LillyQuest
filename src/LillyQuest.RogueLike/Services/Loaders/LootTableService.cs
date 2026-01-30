using LillyQuest.RogueLike.Interfaces;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.LootTables;

namespace LillyQuest.RogueLike.Services.Loaders;

/// <summary>
/// Service for managing loot table definitions.
/// </summary>
public class LootTableService : IDataLoaderReceiver
{
    private readonly Lazy<ItemService>? _itemService;
    private readonly Dictionary<string, LootTableDefinitionJson> _lootTablesById = new();

    public LootTableService(Lazy<ItemService>? itemService)
        => _itemService = itemService;

    public void ClearData()
    {
        _lootTablesById.Clear();
    }

    public Type[] GetLoadTypes()
        => [typeof(LootTableDefinitionJson)];

    public Task LoadDataAsync(List<BaseJsonEntity> entities)
    {
        foreach (var entity in entities.Cast<LootTableDefinitionJson>())
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                continue;
            }

            _lootTablesById[entity.Id] = entity;
        }

        return Task.CompletedTask;
    }

    public bool TryGetLootTable(string lootTableId, out LootTableDefinitionJson lootTable)
    {
        lootTable = null!;

        if (_lootTablesById.TryGetValue(lootTableId, out var resolved))
        {
            lootTable = resolved;

            return true;
        }

        return false;
    }

    public bool VerifyLoadedData()
    {
        foreach (var table in _lootTablesById.Values)
        {
            if (string.IsNullOrWhiteSpace(table.Id))
            {
                throw new InvalidOperationException("LootTable with missing ID found during verification");
            }

            foreach (var entry in table.Entries)
            {
                var hasItem = !string.IsNullOrWhiteSpace(entry.ItemId);
                var hasTable = !string.IsNullOrWhiteSpace(entry.LootTableId);

                if (!hasItem && !hasTable)
                {
                    throw new InvalidOperationException(
                        $"LootTable {table.Id} has entry with neither ItemId nor LootTableId"
                    );
                }

                if (hasItem && hasTable)
                {
                    throw new InvalidOperationException($"LootTable {table.Id} has entry with both ItemId and LootTableId");
                }

                if (entry.Chance < 0f)
                {
                    throw new InvalidOperationException(
                        $"LootTable {table.Id} has entry with negative Chance: {entry.Chance}"
                    );
                }

                if (entry.Chance > 100f)
                {
                    throw new InvalidOperationException($"LootTable {table.Id} has entry with Chance > 100: {entry.Chance}");
                }

                // Validate references
                if (hasItem && _itemService != null)
                {
                    if (!_itemService.Value.TryGetItem(entry.ItemId!, out _))
                    {
                        throw new InvalidOperationException(
                            $"LootTable {table.Id} references missing item '{entry.ItemId}'"
                        );
                    }
                }

                if (hasTable)
                {
                    if (!_lootTablesById.ContainsKey(entry.LootTableId!))
                    {
                        throw new InvalidOperationException(
                            $"LootTable {table.Id} references missing nested loot table '{entry.LootTableId}'"
                        );
                    }
                }
            }
        }

        return true;
    }
}
