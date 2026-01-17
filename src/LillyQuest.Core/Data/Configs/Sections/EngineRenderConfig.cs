namespace LillyQuest.Core.Data.Configs.Sections;

public class EngineRenderConfig
{
    public string Title { get; set; } = "DarkLilly Engine";
    public int Width { get; set; } = 1280;
    public int Height { get; set; } = 720;
    public bool IsResizable { get; set; } = true;
    public bool IsFullscreen { get; set; }
    public bool IsVSyncEnabled { get; set; } = true;

    public int Mssa { get; set; } = 4;
}
