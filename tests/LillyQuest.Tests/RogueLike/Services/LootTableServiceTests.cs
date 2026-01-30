using LillyQuest.RogueLike.Json.Entities.LootTables;
using LillyQuest.RogueLike.Services.Loaders;

namespace LillyQuest.Tests.RogueLike.Services;

public class LootTableServiceTests
{
    [Test]
    public async Task ClearData_RemovesAllLootTables()
    {
        var service = CreateService();
        await service.LoadDataAsync(
            new()
            {
                new LootTableDefinitionJson { Id = "table1", Rolls = "1" }
            }
        );

        service.ClearData();

        var success = service.TryGetLootTable("table1", out _);
        Assert.That(success, Is.False);
    }

    [Test]
    public void GetLoadTypes_ReturnsLootTableDefinitionJson()
    {
        var service = CreateService();

        var types = service.GetLoadTypes();

        Assert.That(types, Has.Length.EqualTo(1));
        Assert.That(types[0], Is.EqualTo(typeof(LootTableDefinitionJson)));
    }

    [Test]
    public async Task TryGetLootTable_ReturnsFalseWhenMissing()
    {
        var service = CreateService();
        await service.LoadDataAsync(new());

        var success = service.TryGetLootTable("missing", out var lootTable);

        Assert.That(success, Is.False);
        Assert.That(lootTable, Is.Null);
    }

    [Test]
    public async Task TryGetLootTable_ReturnsLootTableById()
    {
        var service = CreateService();
        await service.LoadDataAsync(
            new()
            {
                new LootTableDefinitionJson
                {
                    Id = "chest_loot",
                    Rolls = "1d3",
                    Entries =
                    [
                        new() { ItemId = "gold", Chance = 100f, Count = "1d10" }
                    ]
                }
            }
        );

        var success = service.TryGetLootTable("chest_loot", out var lootTable);

        Assert.That(success, Is.True);
        Assert.That(lootTable.Id, Is.EqualTo("chest_loot"));
        Assert.That(lootTable.Rolls, Is.EqualTo("1d3"));
    }

    [Test]
    public async Task VerifyLoadedData_ReturnsTrueForValidData()
    {
        // Service without ItemService to skip item validation
        var service = new LootTableService(null);
        await service.LoadDataAsync(
            new()
            {
                new LootTableDefinitionJson
                {
                    Id = "valid_table",
                    Rolls = "1",
                    Entries =
                    [
                        new() { ItemId = "sword", Chance = 50f }
                    ]
                }
            }
        );

        var result = service.VerifyLoadedData();

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenChanceExceeds100()
    {
        var service = CreateService();
        await service.LoadDataAsync(
            new()
            {
                new LootTableDefinitionJson
                {
                    Id = "invalid_table",
                    Rolls = "1",
                    Entries =
                    [
                        new() { ItemId = "sword", Chance = 150f }
                    ]
                }
            }
        );

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenChanceIsNegative()
    {
        var service = CreateService();
        await service.LoadDataAsync(
            new()
            {
                new LootTableDefinitionJson
                {
                    Id = "invalid_table",
                    Rolls = "1",
                    Entries =
                    [
                        new() { ItemId = "sword", Chance = -10f }
                    ]
                }
            }
        );

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenEntryHasBothItemAndTable()
    {
        var service = CreateService();
        await service.LoadDataAsync(
            new()
            {
                new LootTableDefinitionJson
                {
                    Id = "invalid_table",
                    Rolls = "1",
                    Entries =
                    [
                        new() { ItemId = "sword", LootTableId = "other_table", Chance = 50f }
                    ]
                }
            }
        );

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenEntryHasNoItemOrTable()
    {
        var service = CreateService();
        await service.LoadDataAsync(
            new()
            {
                new LootTableDefinitionJson
                {
                    Id = "invalid_table",
                    Rolls = "1",
                    Entries =
                    [
                        new() { Chance = 50f } // No ItemId or LootTableId
                    ]
                }
            }
        );

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenItemIdNotFound()
    {
        var service = CreateService();
        await service.LoadDataAsync(
            new()
            {
                new LootTableDefinitionJson
                {
                    Id = "chest_loot",
                    Rolls = "1",
                    Entries =
                    [
                        new() { ItemId = "missing_item", Chance = 50f }
                    ]
                }
            }
        );

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenNestedLootTableNotFound()
    {
        var service = CreateService();
        await service.LoadDataAsync(
            new()
            {
                new LootTableDefinitionJson
                {
                    Id = "chest_loot",
                    Rolls = "1",
                    Entries =
                    [
                        new() { LootTableId = "missing_table", Chance = 50f }
                    ]
                }
            }
        );

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }

    private static LootTableService CreateService()
    {
        var lootTableService = new LootTableService(null);
        var itemService = new ItemService(lootTableService);

        return new(new(() => itemService));
    }
}
