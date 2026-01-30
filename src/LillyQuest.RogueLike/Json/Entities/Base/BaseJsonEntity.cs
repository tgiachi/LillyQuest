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
 JsonDerivedType(typeof(ColorSchemaDefintionJson), "color_schema"),
 JsonDerivedType(typeof(TilesetDefinitionJson), "tileset"),
 JsonDerivedType(typeof(TerrainDefinitionJson), "terrain"),
 JsonDerivedType(typeof(CreatureDefinitionJson), "creature"),
 JsonDerivedType(typeof(NameDefinitionJson), "name"),
 JsonDerivedType(typeof(ItemDefinitionJson), "item"),
 JsonDerivedType(typeof(LootTableDefinitionJson), "loot_table")
]
public class BaseJsonEntity
{
    public string Id { get; set; }

    public List<string> Tags { get; set; } = [];

    [JsonPropertyName("--")]
    public string Comment { get; set; }
}
