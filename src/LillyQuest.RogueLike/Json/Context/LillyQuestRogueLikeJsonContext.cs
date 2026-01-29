using System.Text.Json.Serialization;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Colorschemas;
using LillyQuest.RogueLike.Json.Entities.Creatures;
using LillyQuest.RogueLike.Json.Entities.Items;
using LillyQuest.RogueLike.Json.Entities.Names;
using LillyQuest.RogueLike.Json.Entities.Terrain;
using LillyQuest.RogueLike.Json.Entities.Tiles;

namespace LillyQuest.RogueLike.Json.Context;

/// <summary>
/// JSON serializer context for the RogueLike module, providing type-safe serialization and deserialization
/// of tileset definitions, animations, and color schema entities.
/// </summary>
[JsonSerializable(typeof(BaseJsonEntity)),
 JsonSerializable(typeof(List<BaseJsonEntity>)),
 JsonSerializable(typeof(BaseJsonEntity[])),
 JsonSerializable(typeof(TilesetDefinitionJson)),
 JsonSerializable(typeof(TileDefinition)),
 JsonSerializable(typeof(List<TilesetDefinitionJson>)),
 JsonSerializable(typeof(List<TileDefinition>)),
 JsonSerializable(typeof(ColorSchemaJson[])),
 JsonSerializable(typeof(ColorSchemaDefintionJson)),
 JsonSerializable(typeof(TileAnimation)),
 JsonSerializable(typeof(List<TileAnimation>)),
 JsonSerializable(typeof(TileAnimationFrame)),
 JsonSerializable(typeof(TerrainDefinitionJson)),
 JsonSerializable(typeof(TerrainDefinitionJson[])),
 JsonSerializable(typeof(List<TerrainDefinitionJson>)),
 JsonSerializable(typeof(List<TileAnimationFrame>)),
 JsonSerializable(typeof(CreatureDefinitionJson)),
 JsonSerializable(typeof(CreatureDefinitionJson[])),
 JsonSerializable(typeof(List<CreatureDefinitionJson>)),
 JsonSerializable(typeof(NameDefinitionJson)),
 JsonSerializable(typeof(NameDefinitionJson[])),
 JsonSerializable(typeof(List<NameDefinitionJson>)),
 JsonSerializable(typeof(ItemDefintionJson)),
 JsonSerializable(typeof(ItemDefintionJson[])),
 JsonSerializable(typeof(List<ItemDefintionJson>))
]
public partial class LillyQuestRogueLikeJsonContext : JsonSerializerContext { }
