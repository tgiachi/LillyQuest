using System.Text.Json.Serialization;
using LillyQuest.Core.Data.Configs;
using LillyQuest.Core.Data.Configs.Sections;
using LillyQuest.Core.Data.Json.Assets;
using LillyQuest.Core.Types;

namespace LillyQuest.Core.Json;

[JsonSerializable(typeof(LillyQuestEngineConfig)),
 JsonSerializable(typeof(EngineKeyBinding)),
 JsonSerializable(typeof(EngineKeyBinding[])),
 JsonSerializable(typeof(EngineRenderConfig)),
 JsonSerializable(typeof(EngineLoggingConfig)),
 JsonSerializable(typeof(TilesetDefinitionJson)),
]
public partial class JsonEngineContext : JsonSerializerContext
{
    public LogLevelType LogLevel { get; set; } = LogLevelType.Information;

    public bool LogToFile { get; set; }
}
