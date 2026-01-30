using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Components;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Json.Entities.Colorschemas;
using LillyQuest.RogueLike.Json.Entities.Terrain;
using LillyQuest.RogueLike.Json.Entities.Tiles;
using LillyQuest.RogueLike.Services;
using LillyQuest.RogueLike.Services.Loaders;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;

namespace LillyQuest.Tests.RogueLike.Services;

public class MapGeneratorTests
{
    [Test]
    public async Task GenerateMapAsync_PlayerHasTransparentBackground()
    {
        var colorService = new ColorService { DefaultColorSet = "schema" };
        await colorService.LoadDataAsync(
            new()
            {
                new ColorSchemaDefintionJson
                {
                    Id = "schema",
                    Colors = new()
                    {
                        new() { Id = "fg", Color = new(0xFF, 0xFF, 0xFF, 0xFF) },
                        new() { Id = "bg", Color = new(0xFF, 0x00, 0x00, 0x00) }
                    }
                }
            }
        );

        var tileSetService = new TileSetService(colorService);
        await tileSetService.LoadDataAsync(
            new()
            {
                new TilesetDefinitionJson
                {
                    Name = "main",
                    Tiles = new()
                    {
                        new() { Id = "floor", Symbol = ".", FgColor = "fg", BgColor = "bg" },
                        new() { Id = "wall", Symbol = "#", FgColor = "fg", BgColor = "bg" }
                    }
                }
            }
        );

        var terrainService = new TerrainService(tileSetService);
        await terrainService.LoadDataAsync(
            new()
            {
                new TerrainDefinitionJson { Id = "floor", Name = "Floor", Flags = new() { "walkable" } },
                new TerrainDefinitionJson { Id = "wall", Name = "Wall", Flags = new() { "solid" } }
            }
        );

        var mapGenerator = new MapGenerator(terrainService);
        var map = await mapGenerator.GenerateMapAsync();
        var player = map.Entities.GetLayer((int)MapLayer.Creatures).First().Item as CreatureGameObject;

        Assert.That(player, Is.Not.Null);
        Assert.That(player!.Tile.BackgroundColor, Is.EqualTo(LyColor.Transparent));
        Assert.That(player.Tile.ForegroundColor, Is.EqualTo(LyColor.White));
    }

    [Test]
    public async Task GenerateMapAsync_SecondTorchAtFixedPosition()
    {
        var colorService = new ColorService { DefaultColorSet = "schema" };
        await colorService.LoadDataAsync(
            new()
            {
                new ColorSchemaDefintionJson
                {
                    Id = "schema",
                    Colors = new()
                    {
                        new() { Id = "fg", Color = new(0xFF, 0xFF, 0xFF, 0xFF) },
                        new() { Id = "bg", Color = new(0xFF, 0x00, 0x00, 0x00) }
                    }
                }
            }
        );

        var tileSetService = new TileSetService(colorService);
        await tileSetService.LoadDataAsync(
            new()
            {
                new TilesetDefinitionJson
                {
                    Name = "main",
                    Tiles = new()
                    {
                        new() { Id = "floor", Symbol = ".", FgColor = "fg", BgColor = "bg" },
                        new() { Id = "wall", Symbol = "#", FgColor = "fg", BgColor = "bg" }
                    }
                }
            }
        );

        var terrainService = new TerrainService(tileSetService);
        await terrainService.LoadDataAsync(
            new()
            {
                new TerrainDefinitionJson { Id = "floor", Name = "Floor", Flags = new() { "walkable" } },
                new TerrainDefinitionJson { Id = "wall", Name = "Wall", Flags = new() { "solid" } }
            }
        );

        var mapGenerator = new MapGenerator(terrainService);
        var map = await mapGenerator.GenerateMapAsync();

        var torch = map.Entities
                       .GetLayer((int)MapLayer.Items)
                       .FirstOrDefault(entry => entry.Item is ItemGameObject item && item.Position == new Point(8, 6))
                       .Item as ItemGameObject;

        Assert.That(torch, Is.Not.Null);
    }

    [Test]
    public async Task GenerateMapAsync_TorchHasLightBackgroundComponent()
    {
        var colorService = new ColorService { DefaultColorSet = "schema" };
        await colorService.LoadDataAsync(
            new()
            {
                new ColorSchemaDefintionJson
                {
                    Id = "schema",
                    Colors = new()
                    {
                        new() { Id = "fg", Color = new(0xFF, 0xFF, 0xFF, 0xFF) },
                        new() { Id = "bg", Color = new(0xFF, 0x00, 0x00, 0x00) }
                    }
                }
            }
        );

        var tileSetService = new TileSetService(colorService);
        await tileSetService.LoadDataAsync(
            new()
            {
                new TilesetDefinitionJson
                {
                    Name = "main",
                    Tiles = new()
                    {
                        new() { Id = "floor", Symbol = ".", FgColor = "fg", BgColor = "bg" },
                        new() { Id = "wall", Symbol = "#", FgColor = "fg", BgColor = "bg" }
                    }
                }
            }
        );

        var terrainService = new TerrainService(tileSetService);
        await terrainService.LoadDataAsync(
            new()
            {
                new TerrainDefinitionJson { Id = "floor", Name = "Floor", Flags = new() { "walkable" } },
                new TerrainDefinitionJson { Id = "wall", Name = "Wall", Flags = new() { "solid" } }
            }
        );

        var mapGenerator = new MapGenerator(terrainService);
        var map = await mapGenerator.GenerateMapAsync();
        var torch = map.Entities.GetLayer((int)MapLayer.Items).First().Item as ItemGameObject;

        Assert.That(torch, Is.Not.Null);
        Assert.That(torch!.GoRogueComponents.GetFirstOrDefault<LightBackgroundComponent>(), Is.Not.Null);
    }

    [Test]
    public async Task GenerateMapAsync_TorchHasLightComponent()
    {
        var colorService = new ColorService { DefaultColorSet = "schema" };
        await colorService.LoadDataAsync(
            new()
            {
                new ColorSchemaDefintionJson
                {
                    Id = "schema",
                    Colors = new()
                    {
                        new() { Id = "fg", Color = new(0xFF, 0xFF, 0xFF, 0xFF) },
                        new() { Id = "bg", Color = new(0xFF, 0x00, 0x00, 0x00) }
                    }
                }
            }
        );

        var tileSetService = new TileSetService(colorService);
        await tileSetService.LoadDataAsync(
            new()
            {
                new TilesetDefinitionJson
                {
                    Name = "main",
                    Tiles = new()
                    {
                        new() { Id = "floor", Symbol = ".", FgColor = "fg", BgColor = "bg" },
                        new() { Id = "wall", Symbol = "#", FgColor = "fg", BgColor = "bg" }
                    }
                }
            }
        );

        var terrainService = new TerrainService(tileSetService);
        await terrainService.LoadDataAsync(
            new()
            {
                new TerrainDefinitionJson { Id = "floor", Name = "Floor", Flags = new() { "walkable" } },
                new TerrainDefinitionJson { Id = "wall", Name = "Wall", Flags = new() { "solid" } }
            }
        );

        var mapGenerator = new MapGenerator(terrainService);
        var map = await mapGenerator.GenerateMapAsync();
        var torch = map.Entities.GetLayer((int)MapLayer.Items).First().Item as ItemGameObject;

        Assert.That(torch, Is.Not.Null);
        Assert.That(torch!.GoRogueComponents.GetFirstOrDefault<LightSourceComponent>(), Is.Not.Null);
    }
}
