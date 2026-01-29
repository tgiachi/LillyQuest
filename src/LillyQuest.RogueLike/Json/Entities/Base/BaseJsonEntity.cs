using System.Text.Json.Serialization;
using LillyQuest.RogueLike.Json.Entities.Colorschemas;
using LillyQuest.RogueLike.Json.Entities.Creatures;
using LillyQuest.RogueLike.Json.Entities.Items;
using LillyQuest.RogueLike.Json.Entities.LootTables;
using LillyQuest.RogueLike.Json.Entities.Names;
using LillyQuest.RogueLike.Json.Entities.Terrain;
using LillyQuest.RogueLike.Json.Entities.Tiles;

namespace LillyQuest.RogueLike.Json.Entities.Base;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type"),
 JsonDerivedType(typeof(ColorSchemaDefintionJson), typeDiscriminator: "color_schema"),
 JsonDerivedType(typeof(TilesetDefinitionJson), typeDiscriminator: "tileset"),
 JsonDerivedType(typeof(TerrainDefinitionJson), typeDiscriminator: "terrain"),
 JsonDerivedType(typeof(CreatureDefinitionJson), typeDiscriminator: "creature"),
 JsonDerivedType(typeof(NameDefinitionJson), typeDiscriminator: "name"),
 JsonDerivedType(typeof(ItemDefinitionJson), typeDiscriminator: "item"),
 JsonDerivedType(typeof(LootTableDefinitionJson), typeDiscriminator: "loot_table")
]
public class BaseJsonEntity
{
    public string Id { get; set; }

    public List<string> Tags { get; set; } = [];

    [JsonPropertyName("--")]
    public string Comment { get; set; }
}
