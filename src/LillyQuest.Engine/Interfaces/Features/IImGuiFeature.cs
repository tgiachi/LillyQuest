using LillyQuest.Engine.Interfaces.GameObjects.Features;

namespace LillyQuest.Engine.Interfaces.Features;

public interface IImGuiFeature : IGameObjectFeature
{
    string WindowTitle { get; }

    bool IsOpened { get; set; }

    void DrawImGui();
}
