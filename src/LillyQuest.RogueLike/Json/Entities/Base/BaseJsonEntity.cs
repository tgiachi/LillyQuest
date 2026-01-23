using System.Text.Json.Serialization;
using LillyQuest.RogueLike.Json.Entities.Colorschemas;
using LillyQuest.RogueLike.Json.Entities.Terrain;
using LillyQuest.RogueLike.Json.Entities.Tiles;

namespace LillyQuest.RogueLike.Json.Entities.Base;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type"),
 JsonDerivedType(typeof(ColorSchemaDefintionJson), typeDiscriminator: "color_schema"),
 JsonDerivedType(typeof(TilesetDefinitionJson), typeDiscriminator: "tileset"),
 JsonDerivedType(typeof(TerrainDefinitionJson), typeDiscriminator: "terrain")
]
public class BaseJsonEntity
{
    public string Id { get; set; }

    public List<string> Tags { get; set; } = [];
}
