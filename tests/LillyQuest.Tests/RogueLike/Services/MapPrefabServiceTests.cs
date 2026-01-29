using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Prefabs;
using LillyQuest.RogueLike.Services.Loaders;

namespace LillyQuest.Tests.RogueLike.Services;

public class MapPrefabServiceTests
{
    private static MapPrefabService CreateService()
    {
        var colorService = new ColorService();
        var tileSetService = new TileSetService(colorService);
        var terrainService = new TerrainService(tileSetService);
        var lootTableService = new LootTableService(null);
        var itemService = new ItemService(lootTableService);
        var creatureService = new CreatureService();
        return new MapPrefabService(terrainService, itemService, creatureService);
    }

    [Test]
    public async Task TryGetPrefab_ReturnsPrefabById()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new MapPrefabDefinitionJson
            {
                Id = "test_room",
                Category = "dungeon",
                Subcategory = "room",
                Content = new List<string> { "###", "#.#", "###" },
                Palette = new Dictionary<string, MapPaletteEntry>
                {
                    ["#"] = new MapPaletteEntry { Terrain = "wall" },
                    ["."] = new MapPaletteEntry { Terrain = "floor" }
                }
            }
        });

        var success = service.TryGetPrefab("test_room", out var prefab);

        Assert.That(success, Is.True);
        Assert.That(prefab.Id, Is.EqualTo("test_room"));
    }

    [Test]
    public async Task TryGetPrefab_ReturnsFalseWhenMissing()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>());

        var success = service.TryGetPrefab("missing", out var prefab);

        Assert.That(success, Is.False);
        Assert.That(prefab, Is.Null);
    }

    [Test]
    public async Task GetRandomPrefab_ByCategory_ReturnsMatch()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new MapPrefabDefinitionJson
            {
                Id = "dungeon_room",
                Category = "dungeon",
                Subcategory = "room",
                Content = new List<string> { "###" },
                Palette = new Dictionary<string, MapPaletteEntry>()
            },
            new MapPrefabDefinitionJson
            {
                Id = "cave_corridor",
                Category = "cave",
                Subcategory = "corridor",
                Content = new List<string> { "..." },
                Palette = new Dictionary<string, MapPaletteEntry>()
            }
        });

        var prefab = service.GetRandomPrefab("dungeon", null, new Random(42));

        Assert.That(prefab, Is.Not.Null);
        Assert.That(prefab!.Category, Is.EqualTo("dungeon"));
    }

    [Test]
    public async Task GetRandomPrefab_ByCategoryAndSubcategory_ReturnsMatch()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new MapPrefabDefinitionJson
            {
                Id = "dungeon_room_small",
                Category = "dungeon",
                Subcategory = "room",
                Content = new List<string> { "###" },
                Palette = new Dictionary<string, MapPaletteEntry>()
            },
            new MapPrefabDefinitionJson
            {
                Id = "dungeon_corridor",
                Category = "dungeon",
                Subcategory = "corridor",
                Content = new List<string> { "..." },
                Palette = new Dictionary<string, MapPaletteEntry>()
            }
        });

        var prefab = service.GetRandomPrefab("dungeon", "room", new Random(42));

        Assert.That(prefab, Is.Not.Null);
        Assert.That(prefab!.Subcategory, Is.EqualTo("room"));
    }

    [Test]
    public async Task GetRandomPrefab_NoMatches_ReturnsNull()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new MapPrefabDefinitionJson
            {
                Id = "dungeon_room",
                Category = "dungeon",
                Subcategory = "room",
                Content = new List<string> { "###" },
                Palette = new Dictionary<string, MapPaletteEntry>()
            }
        });

        var prefab = service.GetRandomPrefab("cave", null, new Random(42));

        Assert.That(prefab, Is.Null);
    }

    [Test]
    public async Task ClearData_RemovesAllPrefabs()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new MapPrefabDefinitionJson
            {
                Id = "test_prefab",
                Category = "dungeon",
                Subcategory = "room",
                Content = new List<string> { "###" },
                Palette = new Dictionary<string, MapPaletteEntry>()
            }
        });

        service.ClearData();

        var success = service.TryGetPrefab("test_prefab", out _);
        Assert.That(success, Is.False);
    }

    [Test]
    public void GetLoadTypes_ReturnsMapPrefabDefinitionJson()
    {
        var service = CreateService();

        var types = service.GetLoadTypes();

        Assert.That(types, Has.Length.EqualTo(1));
        Assert.That(types[0], Is.EqualTo(typeof(MapPrefabDefinitionJson)));
    }

    [Test]
    public async Task VerifyLoadedData_ReturnsTrueForValidData()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new MapPrefabDefinitionJson
            {
                Id = "test_room",
                Category = "dungeon",
                Subcategory = "room",
                Content = new List<string> { "###", "#.#", "###" },
                Palette = new Dictionary<string, MapPaletteEntry>
                {
                    ["#"] = new MapPaletteEntry { Terrain = "terrain/*" },
                    ["."] = new MapPaletteEntry { Terrain = "terrain/floor" }
                }
            }
        });

        var result = service.VerifyLoadedData();

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenContentEmpty()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new MapPrefabDefinitionJson
            {
                Id = "empty_prefab",
                Category = "dungeon",
                Subcategory = "room",
                Content = new List<string>(),
                Palette = new Dictionary<string, MapPaletteEntry>()
            }
        });

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenPaletteMissing()
    {
        var service = CreateService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new MapPrefabDefinitionJson
            {
                Id = "no_palette",
                Category = "dungeon",
                Subcategory = "room",
                Content = new List<string> { "###" },
                Palette = null!
            }
        });

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenTerrainNotFound()
    {
        var service = CreateService();

        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new MapPrefabDefinitionJson
            {
                Id = "test_prefab",
                Category = "dungeon",
                Subcategory = "room",
                Content = new List<string> { "#" },
                Palette = new Dictionary<string, MapPaletteEntry>
                {
                    ["#"] = new MapPaletteEntry { Terrain = "missing_terrain" }
                }
            }
        });

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenCreatureNotFound()
    {
        var service = CreateService();

        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new MapPrefabDefinitionJson
            {
                Id = "test_prefab",
                Category = "dungeon",
                Subcategory = "room",
                Content = new List<string> { "#" },
                Palette = new Dictionary<string, MapPaletteEntry>
                {
                    ["#"] = new MapPaletteEntry { Creature = "missing_creature" }
                }
            }
        });

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenItemNotFound()
    {
        var service = CreateService();

        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new MapPrefabDefinitionJson
            {
                Id = "test_prefab",
                Category = "dungeon",
                Subcategory = "room",
                Content = new List<string> { "#" },
                Palette = new Dictionary<string, MapPaletteEntry>
                {
                    ["#"] = new MapPaletteEntry { Item = "missing_item" }
                }
            }
        });

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }
}
