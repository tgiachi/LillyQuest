using LillyQuest.Core.Json;
using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Json.Context;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Colorschemas;
using LillyQuest.RogueLike.Json.Entities.Tiles;

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
            QuestLillyRogueLikeJsonContext.Default
        );

        Assert.That(entity.Id, Is.EqualTo("base"));
        Assert.That(entity.Tags, Is.EquivalentTo(new[] { "a", "b" }));
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
            QuestLillyRogueLikeJsonContext.Default
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
            QuestLillyRogueLikeJsonContext.Default
        );

        Assert.That(tilesets.Count, Is.EqualTo(2));
        Assert.That(tilesets[0].Name, Is.EqualTo("a"));
        Assert.That(tilesets[1].TexturePath, Is.EqualTo("b.png"));
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
            QuestLillyRogueLikeJsonContext.Default
        );

        Assert.That(tile.Id, Is.EqualTo("tile-1"));
        Assert.That(tile.Tags, Is.EquivalentTo(new[] { "floor" }));
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
            QuestLillyRogueLikeJsonContext.Default
        );

        Assert.That(schema.Id, Is.EqualTo("color-1"));
        Assert.That(schema.ForegroundColor, Is.EqualTo(new LyColor(0xFF, 0x11, 0x22, 0x33)));
        Assert.That(schema.BackgroundColor, Is.EqualTo(new LyColor(0xCC, 0x44, 0x55, 0x66)));
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
            QuestLillyRogueLikeJsonContext.Default
        );

        Assert.That(definition.Id, Is.EqualTo("schema"));
        Assert.That(definition.Colors.Count, Is.EqualTo(1));
        Assert.That(
            definition.Colors[0].ForegroundColor,
            Is.EqualTo(new LyColor(0xFF, 0x01, 0x02, 0x03))
        );
    }
}
