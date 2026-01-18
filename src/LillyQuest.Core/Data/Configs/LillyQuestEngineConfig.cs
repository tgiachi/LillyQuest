using LillyQuest.Core.Data.Configs.Sections;

namespace LillyQuest.Core.Data.Configs;

public class LillyQuestEngineConfig
{
    public string RootDirectory { get; set; }

    public EngineKeyBinding[] KeyBindings { get; init; } = [];

    public EngineRenderConfig Render { get; init; } = new();

    public EngineLoggingConfig Logging { get; init; } = new();
}
