using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.LootTables;
using LillyQuest.RogueLike.Services.Loaders;

namespace LillyQuest.Tests.RogueLike.Services;

public class LootTableServiceTests
{
    [Test]
    public async Task TryGetLootTable_ReturnsLootTableById()
    {
        var service = new LootTableService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new LootTableDefinitionJson
            {
                Id = "chest_loot",
                Rolls = "1d3",
                Entries = [
                    new LootEntryJson { ItemId = "gold", Chance = 100f, Count = "1d10" }
                ]
            }
        });

        var success = service.TryGetLootTable("chest_loot", out var lootTable);

        Assert.That(success, Is.True);
        Assert.That(lootTable.Id, Is.EqualTo("chest_loot"));
        Assert.That(lootTable.Rolls, Is.EqualTo("1d3"));
    }

    [Test]
    public async Task TryGetLootTable_ReturnsFalseWhenMissing()
    {
        var service = new LootTableService();
        await service.LoadDataAsync(new List<BaseJsonEntity>());

        var success = service.TryGetLootTable("missing", out var lootTable);

        Assert.That(success, Is.False);
        Assert.That(lootTable, Is.Null);
    }

    [Test]
    public async Task ClearData_RemovesAllLootTables()
    {
        var service = new LootTableService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new LootTableDefinitionJson { Id = "table1", Rolls = "1" }
        });

        service.ClearData();

        var success = service.TryGetLootTable("table1", out _);
        Assert.That(success, Is.False);
    }

    [Test]
    public void GetLoadTypes_ReturnsLootTableDefinitionJson()
    {
        var service = new LootTableService();

        var types = service.GetLoadTypes();

        Assert.That(types, Has.Length.EqualTo(1));
        Assert.That(types[0], Is.EqualTo(typeof(LootTableDefinitionJson)));
    }

    [Test]
    public async Task VerifyLoadedData_ReturnsTrueForValidData()
    {
        var service = new LootTableService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new LootTableDefinitionJson
            {
                Id = "valid_table",
                Rolls = "1",
                Entries = [
                    new LootEntryJson { ItemId = "sword", Chance = 50f }
                ]
            }
        });

        var result = service.VerifyLoadedData();

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenEntryHasNoItemOrTable()
    {
        var service = new LootTableService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new LootTableDefinitionJson
            {
                Id = "invalid_table",
                Rolls = "1",
                Entries = [
                    new LootEntryJson { Chance = 50f }  // No ItemId or LootTableId
                ]
            }
        });

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenEntryHasBothItemAndTable()
    {
        var service = new LootTableService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new LootTableDefinitionJson
            {
                Id = "invalid_table",
                Rolls = "1",
                Entries = [
                    new LootEntryJson { ItemId = "sword", LootTableId = "other_table", Chance = 50f }
                ]
            }
        });

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenChanceIsNegative()
    {
        var service = new LootTableService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new LootTableDefinitionJson
            {
                Id = "invalid_table",
                Rolls = "1",
                Entries = [
                    new LootEntryJson { ItemId = "sword", Chance = -10f }
                ]
            }
        });

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenChanceExceeds100()
    {
        var service = new LootTableService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new LootTableDefinitionJson
            {
                Id = "invalid_table",
                Rolls = "1",
                Entries = [
                    new LootEntryJson { ItemId = "sword", Chance = 150f }
                ]
            }
        });

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }
}
