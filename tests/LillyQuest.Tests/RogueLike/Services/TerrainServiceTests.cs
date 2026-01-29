using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Data.Internal;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Colorschemas;
using LillyQuest.RogueLike.Json.Entities.Terrain;
using LillyQuest.RogueLike.Json.Entities.Tiles;
using LillyQuest.RogueLike.Services;
using System.Linq;
using LillyQuest.RogueLike.Services.Loaders;

namespace LillyQuest.Tests.RogueLike.Services;

public class TerrainServiceTests
{
    [Test]
    public void ResolvedTerrainData_StoresAllFields()
    {
        var fg = new LyColor(0xFF, 0x01, 0x02, 0x03);
        var bg = new LyColor(0xFF, 0x10, 0x20, 0x30);
        var flags = new List<string> { "walkable" };
        var tags = new List<string> { "floor" };

        var data = new ResolvedTerrainData(
            "floor",
            "Floor",
            "Stone floor",
            flags,
            1,
            "comment",
            tags,
            "terrain",
            "basic",
            ".",
            fg,
            bg,
            null
        );

        Assert.That(data.Id, Is.EqualTo("floor"));
        Assert.That(data.Name, Is.EqualTo("Floor"));
        Assert.That(data.Description, Is.EqualTo("Stone floor"));
        Assert.That(data.Flags, Is.EquivalentTo(flags));
        Assert.That(data.MovementCost, Is.EqualTo(1));
        Assert.That(data.Comment, Is.EqualTo("comment"));
        Assert.That(data.Tags, Is.EquivalentTo(tags));
        Assert.That(data.Category, Is.EqualTo("terrain"));
        Assert.That(data.Subcategory, Is.EqualTo("basic"));
        Assert.That(data.TileSymbol, Is.EqualTo("."));
        Assert.That(data.TileFgColor, Is.EqualTo(fg));
        Assert.That(data.TileBgColor, Is.EqualTo(bg));
    }

    [Test]
    public async Task TryGetTerrain_ReturnsResolvedTerrain()
    {
        var colorService = new ColorService { DefaultColorSet = "schema" };
        await colorService.LoadDataAsync(new List<BaseJsonEntity>
        {
            new ColorSchemaDefintionJson
            {
                Id = "schema",
                Colors = new List<ColorSchemaJson>
                {
                    new ColorSchemaJson { Id = "fg", Color = new LyColor(0xFF, 0x01, 0x02, 0x03) },
                    new ColorSchemaJson { Id = "bg", Color = new LyColor(0xFF, 0x10, 0x20, 0x30) }
                }
            }
        });

        var tileSetService = new TileSetService(colorService);
        await tileSetService.LoadDataAsync(new List<BaseJsonEntity>
        {
            new TilesetDefinitionJson
            {
                Name = "main",
                Tiles = new List<TileDefinition>
                {
                    new TileDefinition { Id = "floor", Symbol = ".", FgColor = "fg", BgColor = "bg" }
                }
            }
        });

        var terrainService = new TerrainService(tileSetService);
        await terrainService.LoadDataAsync(new List<BaseJsonEntity>
        {
            new TerrainDefinitionJson
            {
                Id = "floor",
                Name = "Floor",
                Description = "Stone floor",
                Flags = new List<string> { "walkable" },
                MovementCost = 1
            }
        });

        var success = terrainService.TryGetTerrain("floor", out var terrain);

        Assert.That(success, Is.True);
        Assert.That(terrain.Id, Is.EqualTo("floor"));
        Assert.That(terrain.TileSymbol, Is.EqualTo("."));
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenTileMissing()
    {
        var terrainService = new TerrainService(new TileSetService(new ColorService()));
        await terrainService.LoadDataAsync(new List<BaseJsonEntity>
        {
            new TerrainDefinitionJson { Id = "unknown", Name = "Unknown" }
        });

        Assert.Throws<InvalidOperationException>(() => terrainService.VerifyLoadedData());
    }

    [Test]
    public async Task TryGetTerrainByIdOrTag_ReturnsResolvedTerrainForId()
    {
        var terrainService = await CreateTerrainServiceAsync(
            new TerrainDefinitionJson
            {
                Id = "floor",
                Name = "Floor",
                Description = "Stone floor",
                Flags = new List<string> { "walkable" },
                MovementCost = 1,
                Tags = new List<string> { "ground" }
            }
        );

        var success = terrainService.TryGetTerrainByIdOrTag("floor", out var terrain, out var taggedTerrains);

        Assert.That(success, Is.True);
        Assert.That(terrain, Is.Not.Null);
        Assert.That(terrain.Id, Is.EqualTo("floor"));
        Assert.That(taggedTerrains, Is.Empty);
    }

    [Test]
    public async Task TryGetTerrainByIdOrTag_ReturnsListForTag_CaseInsensitive()
    {
        var terrainService = await CreateTerrainServiceAsync(
            new TerrainDefinitionJson
            {
                Id = "floor",
                Name = "Floor",
                Description = "Stone floor",
                Flags = new List<string> { "walkable" },
                MovementCost = 1,
                Tags = new List<string> { "ground", "surface" }
            },
            new TerrainDefinitionJson
            {
                Id = "dirt",
                Name = "Dirt",
                Description = "Dirt floor",
                Flags = new List<string> { "walkable" },
                MovementCost = 2,
                Tags = new List<string> { "Ground" }
            }
        );

        var success = terrainService.TryGetTerrainByIdOrTag("GROUND", out var terrain, out var taggedTerrains);

        Assert.That(success, Is.True);
        Assert.That(terrain, Is.Null);
        Assert.That(taggedTerrains, Has.Count.EqualTo(2));
        Assert.That(taggedTerrains.Select(t => t.Id), Is.EquivalentTo(new[] { "floor", "dirt" }));
    }

    [Test]
    public async Task TryGetTerrainByIdOrTag_ReturnsFalseWhenMissing()
    {
        var terrainService = await CreateTerrainServiceAsync(
            new TerrainDefinitionJson
            {
                Id = "floor",
                Name = "Floor",
                Description = "Stone floor",
                Flags = new List<string> { "walkable" },
                MovementCost = 1,
                Tags = new List<string> { "ground" }
            }
        );

        var success = terrainService.TryGetTerrainByIdOrTag("unknown", out var terrain, out var taggedTerrains);

        Assert.That(success, Is.False);
        Assert.That(terrain, Is.Null);
        Assert.That(taggedTerrains, Is.Empty);
    }

    [Test]
    public async Task GetRandomTerrains_ByCategory_ReturnsRequestedCount()
    {
        var terrainService = await CreateTerrainServiceAsync(
            new TerrainDefinitionJson
            {
                Id = "forest_dense",
                Name = "Forest Dense",
                Description = "Dense forest",
                MovementCost = 2,
                Category = "forest",
                Subcategory = "dense"
            },
            new TerrainDefinitionJson
            {
                Id = "forest_edge",
                Name = "Forest Edge",
                Description = "Sparse forest",
                MovementCost = 1,
                Category = "forest",
                Subcategory = "edge"
            },
            new TerrainDefinitionJson
            {
                Id = "desert",
                Name = "Desert",
                Description = "Hot sand",
                MovementCost = 2,
                Category = "desert",
                Subcategory = "sand"
            }
        );

        var result = terrainService.GetRandomTerrains("forest", null, 2, new Random(7));

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.All(t => t.Category.Equals("forest", StringComparison.OrdinalIgnoreCase)), Is.True);
    }

    [Test]
    public async Task GetRandomTerrains_ByCategoryAndSubcategory_RespectsBothFilters()
    {
        var terrainService = await CreateTerrainServiceAsync(
            new TerrainDefinitionJson
            {
                Id = "forest_dense",
                Name = "Forest Dense",
                Description = "Dense forest",
                MovementCost = 2,
                Category = "forest",
                Subcategory = "dense"
            },
            new TerrainDefinitionJson
            {
                Id = "swamp_dense",
                Name = "Swamp Dense",
                Description = "Dense swamp",
                MovementCost = 3,
                Category = "swamp",
                Subcategory = "dense"
            }
        );

        var result = terrainService.GetRandomTerrains("forest", "dense", 10, new Random(3));

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Category, Is.EqualTo("forest"));
        Assert.That(result[0].Subcategory, Is.EqualTo("dense"));
    }

    [Test]
    public async Task GetRandomTerrains_ByWildcardSubcategory_UsesContainsMatch()
    {
        var terrainService = await CreateTerrainServiceAsync(
            new TerrainDefinitionJson
            {
                Id = "forest_dense",
                Name = "Forest Dense",
                Description = "Dense forest",
                MovementCost = 2,
                Category = "forest",
                Subcategory = "dense"
            },
            new TerrainDefinitionJson
            {
                Id = "forest_thin",
                Name = "Forest Thin",
                Description = "Thin forest",
                MovementCost = 1,
                Category = "forest",
                Subcategory = "thin"
            }
        );

        var result = terrainService.GetRandomTerrains("forest", "*en*", 10, new Random(5));

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Subcategory, Is.EqualTo("dense"));
    }

    [Test]
    public async Task GetRandomTerrains_WithNonPositiveCount_ReturnsEmpty()
    {
        var terrainService = await CreateTerrainServiceAsync(
            new TerrainDefinitionJson
            {
                Id = "forest_dense",
                Name = "Forest Dense",
                Description = "Dense forest",
                MovementCost = 2,
                Category = "forest",
                Subcategory = "dense"
            }
        );

        var result = terrainService.GetRandomTerrains("forest", null, 0, new Random(1));

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetRandomTerrains_WithNoMatches_ReturnsEmpty()
    {
        var terrainService = await CreateTerrainServiceAsync(
            new TerrainDefinitionJson
            {
                Id = "forest_dense",
                Name = "Forest Dense",
                Description = "Dense forest",
                MovementCost = 2,
                Category = "forest",
                Subcategory = "dense"
            }
        );

        var result = terrainService.GetRandomTerrains("desert", null, 2, new Random(2));

        Assert.That(result, Is.Empty);
    }

    private static async Task<TerrainService> CreateTerrainServiceAsync(params TerrainDefinitionJson[] terrains)
    {
        var colorService = new ColorService { DefaultColorSet = "schema" };
        await colorService.LoadDataAsync(new List<BaseJsonEntity>
        {
            new ColorSchemaDefintionJson
            {
                Id = "schema",
                Colors = new List<ColorSchemaJson>
                {
                    new ColorSchemaJson { Id = "fg", Color = new LyColor(0xFF, 0x01, 0x02, 0x03) },
                    new ColorSchemaJson { Id = "bg", Color = new LyColor(0xFF, 0x10, 0x20, 0x30) }
                }
            }
        });

        var tileSetService = new TileSetService(colorService);
        await tileSetService.LoadDataAsync(new List<BaseJsonEntity>
        {
            new TilesetDefinitionJson
            {
                Name = "main",
                Tiles = new List<TileDefinition>
                {
                    new TileDefinition { Id = "floor", Symbol = ".", FgColor = "fg", BgColor = "bg" },
                    new TileDefinition { Id = "dirt", Symbol = ",", FgColor = "fg", BgColor = "bg" },
                    new TileDefinition { Id = "forest_dense", Symbol = "F", FgColor = "fg", BgColor = "bg" },
                    new TileDefinition { Id = "forest_edge", Symbol = "f", FgColor = "fg", BgColor = "bg" },
                    new TileDefinition { Id = "forest_thin", Symbol = "t", FgColor = "fg", BgColor = "bg" },
                    new TileDefinition { Id = "swamp_dense", Symbol = "s", FgColor = "fg", BgColor = "bg" },
                    new TileDefinition { Id = "desert", Symbol = "d", FgColor = "fg", BgColor = "bg" }
                }
            }
        });

        var terrainService = new TerrainService(tileSetService);
        await terrainService.LoadDataAsync(terrains.Cast<BaseJsonEntity>().ToList());
        return terrainService;
    }
}
