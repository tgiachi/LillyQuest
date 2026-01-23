using System.Text.Json.Serialization;
using LillyQuest.RogueLike.Json.Entities.Tiles;

namespace LillyQuest.RogueLike.Json.Context;

[JsonSerializable(typeof(TilesetDefinitionJson))]
public partial class QuestLillyRogueLikeJsonContext : JsonSerializerContext { }
