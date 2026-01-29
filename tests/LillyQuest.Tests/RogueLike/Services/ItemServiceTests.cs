using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Items;
using LillyQuest.RogueLike.Services.Loaders;
using LillyQuest.RogueLike.Types;

namespace LillyQuest.Tests.RogueLike.Services;

public class ItemServiceTests
{
    private static ItemService CreateService()
    {
        var lootTableService = new LootTableService((Lazy<ItemService>?)null);
        return new ItemService(lootTableService);
    }

    [Test]
    public async Task TryGetItem_ReturnsItemById()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new ItemDefinitionJson
            {
                Id = "longsword",
                Name = "Longsword",
                Category = "weapon",
                Subcategory = "sword"
            }
        });

        var success = service.TryGetItem("longsword", out var item);

        Assert.That(success, Is.True);
        Assert.That(item.Id, Is.EqualTo("longsword"));
    }

    [Test]
    public async Task TryGetItem_ReturnsFalseWhenMissing()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>());

        var success = service.TryGetItem("missing", out var item);

        Assert.That(success, Is.False);
        Assert.That(item, Is.Null);
    }

    [Test]
    public async Task GetRandomItem_ByCategory_ReturnsMatch()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new ItemDefinitionJson
            {
                Id = "longsword",
                Category = "weapon",
                Subcategory = "sword"
            },
            new ItemDefinitionJson
            {
                Id = "healing_potion",
                Category = "potion",
                Subcategory = "healing"
            },
            new ItemDefinitionJson
            {
                Id = "dagger",
                Category = "weapon",
                Subcategory = "dagger"
            }
        });

        var item = service.GetRandomItem("weapon", null, new Random(42));

        Assert.That(item, Is.Not.Null);
        Assert.That(item!.Category, Is.EqualTo("weapon"));
    }

    [Test]
    public async Task GetRandomItem_ByCategoryAndSubcategory_ReturnsMatch()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new ItemDefinitionJson
            {
                Id = "longsword",
                Category = "weapon",
                Subcategory = "sword"
            },
            new ItemDefinitionJson
            {
                Id = "shortsword",
                Category = "weapon",
                Subcategory = "sword"
            },
            new ItemDefinitionJson
            {
                Id = "dagger",
                Category = "weapon",
                Subcategory = "dagger"
            }
        });

        var item = service.GetRandomItem("weapon", "sword", new Random(42));

        Assert.That(item, Is.Not.Null);
        Assert.That(item!.Subcategory, Is.EqualTo("sword"));
    }

    [Test]
    public async Task GetRandomItem_WithWildcard_ReturnsMatch()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new ItemDefinitionJson
            {
                Id = "healing_potion_minor",
                Category = "potion",
                Subcategory = "healing-minor"
            },
            new ItemDefinitionJson
            {
                Id = "healing_potion_major",
                Category = "potion",
                Subcategory = "healing-major"
            },
            new ItemDefinitionJson
            {
                Id = "mana_potion",
                Category = "potion",
                Subcategory = "mana"
            }
        });

        var item = service.GetRandomItem("potion", "*healing*", new Random(42));

        Assert.That(item, Is.Not.Null);
        Assert.That(item!.Subcategory, Does.Contain("healing"));
    }

    [Test]
    public async Task GetRandomItem_NoMatches_ReturnsNull()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new ItemDefinitionJson
            {
                Id = "longsword",
                Category = "weapon",
                Subcategory = "sword"
            }
        });

        var item = service.GetRandomItem("armor", null, new Random(42));

        Assert.That(item, Is.Null);
    }

    [Test]
    public async Task ClearData_RemovesAllItems()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new ItemDefinitionJson { Id = "item1", Category = "weapon", Subcategory = "sword" }
        });

        service.ClearData();

        var success = service.TryGetItem("item1", out _);
        Assert.That(success, Is.False);
    }

    [Test]
    public void GetLoadTypes_ReturnsItemDefinitionJson()
    {
        var service = CreateService();

        var types = service.GetLoadTypes();

        Assert.That(types, Has.Length.EqualTo(1));
        Assert.That(types[0], Is.EqualTo(typeof(ItemDefinitionJson)));
    }

    [Test]
    public async Task VerifyLoadedData_ReturnsTrueForValidData()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new ItemDefinitionJson
            {
                Id = "longsword",
                Name = "Longsword",
                Category = "weapon",
                Subcategory = "sword",
                Slot = EquipmentSlot.MainHand
            }
        });

        var result = service.VerifyLoadedData();

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenSlotInvalid()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new ItemDefinitionJson
            {
                Id = "broken_item",
                Category = "weapon",
                Subcategory = "sword",
                Slot = (EquipmentSlot)999
            }
        });

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenContainerHasNoCapacity()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new ItemDefinitionJson
            {
                Id = "broken_container",
                Category = "container",
                Subcategory = "bag",
                IsContainer = true,
                Capacity = null  // Container without capacity
            }
        });

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenLootTableNotFound()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new ItemDefinitionJson
            {
                Id = "container_with_loot",
                Category = "container",
                Subcategory = "bag",
                IsContainer = true,
                Capacity = 10f,
                LootTable = "missing_loot_table"
            }
        });

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }
}
