using LillyQuest.Core.Json;
using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Json.Context;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Colorschemas;
using LillyQuest.RogueLike.Json.Entities.Tiles;
using LillyQuest.RogueLike.Types;

namespace LillyQuest.Tests.RogueLike.Json;

public class RogueLikeJsonSerializationTests
{
    [Test]
    public void Deserialize_BaseJsonEntity_UsesContext()
    {
        const string json = """
                            {
                              "id": "base",
                              "tags": ["a", "b"]
                            }
                            """;

        var entity = JsonUtils.Deserialize<BaseJsonEntity>(
            json,
            LillyQuestRogueLikeJsonContext.Default
        );

        Assert.That(entity.Id, Is.EqualTo("base"));
        Assert.That(entity.Tags, Is.EquivalentTo(new[] { "a", "b" }));
    }

    [Test]
    public void Deserialize_ColorSchemaDefinition_UsesContext()
    {
        const string json = """
                            {
                              "id": "schema",
                              "colors": [
                                {
                                  "id": "color-1",
                                  "foregroundColor": "#FF010203",
                                  "backgroundColor": "#FF0A0B0C"
                                }
                              ]
                            }
                            """;

        var definition = JsonUtils.Deserialize<ColorSchemaDefintionJson>(
            json,
            LillyQuestRogueLikeJsonContext.Default
        );

        Assert.That(definition.Id, Is.EqualTo("schema"));
        Assert.That(definition.Colors.Count, Is.EqualTo(1));
        Assert.That(
            definition.Colors[0].ForegroundColor,
            Is.EqualTo(new LyColor(0xFF, 0x01, 0x02, 0x03))
        );
    }

    [Test]
    public void Deserialize_ColorSchemaJson_UsesContext()
    {
        const string json = """
                            {
                              "id": "color-1",
                              "foregroundColor": "#FF112233",
                              "backgroundColor": "#CC445566"
                            }
                            """;

        var schema = JsonUtils.Deserialize<ColorSchemaJson>(
            json,
            LillyQuestRogueLikeJsonContext.Default
        );

        Assert.That(schema.Id, Is.EqualTo("color-1"));
        Assert.That(schema.ForegroundColor, Is.EqualTo(new LyColor(0xFF, 0x11, 0x22, 0x33)));
        Assert.That(schema.BackgroundColor, Is.EqualTo(new LyColor(0xCC, 0x44, 0x55, 0x66)));
    }

    [Test]
    public void Deserialize_TileDefinition_UsesContext()
    {
        const string json = """
                            {
                              "id": "tile-1",
                              "tags": ["floor"]
                            }
                            """;

        var tile = JsonUtils.Deserialize<TileDefinition>(
            json,
            LillyQuestRogueLikeJsonContext.Default
        );

        Assert.That(tile.Id, Is.EqualTo("tile-1"));
        Assert.That(tile.Tags, Is.EquivalentTo(new[] { "floor" }));
    }

    [Test]
    public void Deserialize_TilesetDefinition_UsesContext()
    {
        const string json = """
                            {
                              "name": "tileset",
                              "texturePath": "tiles.png"
                            }
                            """;

        var tileset = JsonUtils.Deserialize<TilesetDefinitionJson>(
            json,
            LillyQuestRogueLikeJsonContext.Default
        );

        Assert.That(tileset.Name, Is.EqualTo("tileset"));
        Assert.That(tileset.TexturePath, Is.EqualTo("tiles.png"));
    }

    [Test]
    public void Deserialize_TilesetDefinitionList_UsesContext()
    {
        const string json = """
                            [
                              { "name": "a", "texturePath": "a.png" },
                              { "name": "b", "texturePath": "b.png" }
                            ]
                            """;

        var tilesets = JsonUtils.Deserialize<List<TilesetDefinitionJson>>(
            json,
            LillyQuestRogueLikeJsonContext.Default
        );

        Assert.That(tilesets.Count, Is.EqualTo(2));
        Assert.That(tilesets[0].Name, Is.EqualTo("a"));
        Assert.That(tilesets[1].TexturePath, Is.EqualTo("b.png"));
    }

    [Test]
    public void Deserialize_TileAnimation_UsesContext()
    {
        const string json = """
                            {
                              "type": "pingPong",
                              "frameDurationMs": 120,
                              "frames": [
                                { "symbol": ".", "fgColor": "#FF112233", "bgColor": "#FF445566" },
                                { "symbol": ",", "fgColor": "#FF778899" }
                              ]
                            }
                            """;

        var animation = JsonUtils.Deserialize<TileAnimation>(
            json,
            LillyQuestRogueLikeJsonContext.Default
        );

        Assert.That(animation.Type, Is.EqualTo(TileAnimationType.PingPong));
        Assert.That(animation.FrameDurationMs, Is.EqualTo(120));
        Assert.That(animation.Frames.Count, Is.EqualTo(2));
        Assert.That(animation.Frames[0].Symbol, Is.EqualTo("."));
        Assert.That(animation.Frames[0].FgColor, Is.EqualTo("#FF112233"));
        Assert.That(animation.Frames[0].BgColor, Is.EqualTo("#FF445566"));
        Assert.That(animation.Frames[1].Symbol, Is.EqualTo(","));
        Assert.That(animation.Frames[1].BgColor, Is.Null);
    }

    [Test]
    public void Deserialize_TileDefinition_WithAnimation_UsesContext()
    {
        const string json = """
                            {
                              "id": "tile-animated",
                              "tags": ["animated"],
                              "symbol": "@",
                              "fgColor": "#FFFFFFFF",
                              "bgColor": "#FF000000",
                              "animation": {
                                "type": "loop",
                                "frameDurationMs": 75,
                                "frames": [
                                  { "symbol": "A" },
                                  { "symbol": "B", "fgColor": "#FF00FF00" }
                                ]
                              }
                            }
                            """;

        var tile = JsonUtils.Deserialize<TileDefinition>(
            json,
            LillyQuestRogueLikeJsonContext.Default
        );

        Assert.That(tile.Id, Is.EqualTo("tile-animated"));
        Assert.That(tile.Tags, Is.EquivalentTo(new[] { "animated" }));
        Assert.That(tile.Symbol, Is.EqualTo("@"));
        Assert.That(tile.FgColor, Is.EqualTo("#FFFFFFFF"));
        Assert.That(tile.BgColor, Is.EqualTo("#FF000000"));
        Assert.That(tile.Animation, Is.Not.Null);
        Assert.That(tile.Animation!.Type, Is.EqualTo(TileAnimationType.Loop));
        Assert.That(tile.Animation.FrameDurationMs, Is.EqualTo(75));
        Assert.That(tile.Animation.Frames.Count, Is.EqualTo(2));
        Assert.That(tile.Animation.Frames[1].Symbol, Is.EqualTo("B"));
        Assert.That(tile.Animation.Frames[1].FgColor, Is.EqualTo("#FF00FF00"));
    }

    [Test]
    public void Deserialize_TilesetDefinition_WithTiles_UsesContext()
    {
        const string json = """
                            {
                              "name": "animated-tiles",
                              "texturePath": "tiles.png",
                              "tiles": [
                                {
                                  "id": "tile-1",
                                  "tags": ["floor"],
                                  "symbol": ".",
                                  "fgColor": "#FFAAAAAA",
                                  "bgColor": "#FF111111"
                                },
                                {
                                  "id": "tile-2",
                                  "tags": ["lava"],
                                  "symbol": "~",
                                  "fgColor": "#FFFF3300",
                                  "bgColor": "#FF220000",
                                  "animation": {
                                    "type": "once",
                                    "frameDurationMs": 200,
                                    "frames": [
                                      { "symbol": "~" },
                                      { "symbol": "^" }
                                    ]
                                  }
                                }
                              ]
                            }
                            """;

        var tileset = JsonUtils.Deserialize<TilesetDefinitionJson>(
            json,
            LillyQuestRogueLikeJsonContext.Default
        );

        Assert.That(tileset.Name, Is.EqualTo("animated-tiles"));
        Assert.That(tileset.TexturePath, Is.EqualTo("tiles.png"));
        Assert.That(tileset.Tiles.Count, Is.EqualTo(2));
        Assert.That(tileset.Tiles[0].Id, Is.EqualTo("tile-1"));
        Assert.That(tileset.Tiles[1].Animation, Is.Not.Null);
        Assert.That(tileset.Tiles[1].Animation!.Type, Is.EqualTo(TileAnimationType.Once));
        Assert.That(tileset.Tiles[1].Animation.Frames.Count, Is.EqualTo(2));
    }
}
