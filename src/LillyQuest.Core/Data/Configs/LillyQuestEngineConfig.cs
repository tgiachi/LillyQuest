using LillyQuest.Core.Data.Configs.Sections;

namespace LillyQuest.Core.Data.Configs;

public class LillyQuestEngineConfig
{
    public string RootDirectory { get; set; }

    public EngineKeyBinding[] KeyBindings { get; init; } = [];

    public EngineRenderConfig Render { get; init; } = new();

    public EngineLoggingConfig Logging { get; init; } = new();

    /// <summary>
    /// Fixed timestep in seconds for FixedUpdate calls. Default is 1/60 seconds (60 Hz).
    /// </summary>
    public double FixedTimestep { get; init; } = 1.0 / 60.0;
}
