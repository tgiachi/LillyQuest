using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Components;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Colorschemas;
using LillyQuest.RogueLike.Json.Entities.Terrain;
using LillyQuest.RogueLike.Json.Entities.Tiles;
using LillyQuest.RogueLike.Services;
using LillyQuest.RogueLike.Types;

namespace LillyQuest.Tests.RogueLike.Services;

public class MapGeneratorTests
{
    [Test]
    public async Task GenerateMapAsync_PlayerHasTransparentBackground()
    {
        var colorService = new ColorService { DefaultColorSet = "schema" };
        await colorService.LoadDataAsync(new List<BaseJsonEntity>
        {
            new ColorSchemaDefintionJson
            {
                Id = "schema",
                Colors = new List<ColorSchemaJson>
                {
                    new ColorSchemaJson { Id = "fg", Color = new LyColor(0xFF, 0xFF, 0xFF, 0xFF) },
                    new ColorSchemaJson { Id = "bg", Color = new LyColor(0xFF, 0x00, 0x00, 0x00) }
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
                    new TileDefinition { Id = "wall", Symbol = "#", FgColor = "fg", BgColor = "bg" }
                }
            }
        });

        var terrainService = new TerrainService(tileSetService);
        await terrainService.LoadDataAsync(new List<BaseJsonEntity>
        {
            new TerrainDefinitionJson { Id = "floor", Name = "Floor", Flags = new List<string> { "walkable" } },
            new TerrainDefinitionJson { Id = "wall", Name = "Wall", Flags = new List<string> { "solid" } }
        });

        var mapGenerator = new MapGenerator(terrainService);
        var map = await mapGenerator.GenerateMapAsync();
        var player = map.Entities.GetLayer((int)MapLayer.Creatures).First().Item as CreatureGameObject;

        Assert.That(player, Is.Not.Null);
        Assert.That(player!.Tile.BackgroundColor, Is.EqualTo(LyColor.Transparent));
        Assert.That(player.Tile.ForegroundColor, Is.EqualTo(LyColor.White));
    }

    [Test]
    public async Task GenerateMapAsync_TorchHasLightComponent()
    {
        var colorService = new ColorService { DefaultColorSet = "schema" };
        await colorService.LoadDataAsync(new List<BaseJsonEntity>
        {
            new ColorSchemaDefintionJson
            {
                Id = "schema",
                Colors = new List<ColorSchemaJson>
                {
                    new ColorSchemaJson { Id = "fg", Color = new LyColor(0xFF, 0xFF, 0xFF, 0xFF) },
                    new ColorSchemaJson { Id = "bg", Color = new LyColor(0xFF, 0x00, 0x00, 0x00) }
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
                    new TileDefinition { Id = "wall", Symbol = "#", FgColor = "fg", BgColor = "bg" }
                }
            }
        });

        var terrainService = new TerrainService(tileSetService);
        await terrainService.LoadDataAsync(new List<BaseJsonEntity>
        {
            new TerrainDefinitionJson { Id = "floor", Name = "Floor", Flags = new List<string> { "walkable" } },
            new TerrainDefinitionJson { Id = "wall", Name = "Wall", Flags = new List<string> { "solid" } }
        });

        var mapGenerator = new MapGenerator(terrainService);
        var map = await mapGenerator.GenerateMapAsync();
        var torch = map.Entities.GetLayer((int)MapLayer.Items).First().Item as ItemGameObject;

        Assert.That(torch, Is.Not.Null);
        Assert.That(torch!.GoRogueComponents.GetFirstOrDefault<LightSourceComponent>(), Is.Not.Null);
    }

    [Test]
    public async Task GenerateMapAsync_TorchHasLightBackgroundComponent()
    {
        var colorService = new ColorService { DefaultColorSet = "schema" };
        await colorService.LoadDataAsync(new List<BaseJsonEntity>
        {
            new ColorSchemaDefintionJson
            {
                Id = "schema",
                Colors = new List<ColorSchemaJson>
                {
                    new ColorSchemaJson { Id = "fg", Color = new LyColor(0xFF, 0xFF, 0xFF, 0xFF) },
                    new ColorSchemaJson { Id = "bg", Color = new LyColor(0xFF, 0x00, 0x00, 0x00) }
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
                    new TileDefinition { Id = "wall", Symbol = "#", FgColor = "fg", BgColor = "bg" }
                }
            }
        });

        var terrainService = new TerrainService(tileSetService);
        await terrainService.LoadDataAsync(new List<BaseJsonEntity>
        {
            new TerrainDefinitionJson { Id = "floor", Name = "Floor", Flags = new List<string> { "walkable" } },
            new TerrainDefinitionJson { Id = "wall", Name = "Wall", Flags = new List<string> { "solid" } }
        });

        var mapGenerator = new MapGenerator(terrainService);
        var map = await mapGenerator.GenerateMapAsync();
        var torch = map.Entities.GetLayer((int)MapLayer.Items).First().Item as ItemGameObject;

        Assert.That(torch, Is.Not.Null);
        Assert.That(torch!.GoRogueComponents.GetFirstOrDefault<LightBackgroundComponent>(), Is.Not.Null);
    }
}
