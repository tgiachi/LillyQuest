using System.Text.Json.Serialization;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Colorschemas;
using LillyQuest.RogueLike.Json.Entities.Tiles;

namespace LillyQuest.RogueLike.Json.Context;

[JsonSerializable(typeof(BaseJsonEntity)),
 JsonSerializable(typeof(TilesetDefinitionJson)),
 JsonSerializable(typeof(TileDefinition)),
 JsonSerializable(typeof(List<TilesetDefinitionJson>)),
 JsonSerializable(typeof(List<TileDefinition>)),
 JsonSerializable(typeof(ColorSchemaJson[])),
 JsonSerializable(typeof(ColorSchemaDefintionJson))
]
public partial class QuestLillyRogueLikeJsonContext : JsonSerializerContext { }
