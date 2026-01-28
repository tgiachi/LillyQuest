using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Data.Internal;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Colorschemas;
using LillyQuest.RogueLike.Json.Entities.Terrain;
using LillyQuest.RogueLike.Json.Entities.Tiles;
using LillyQuest.RogueLike.Services;

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
}
