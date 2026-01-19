using LillyQuest.Engine.Interfaces.Systems;

namespace LillyQuest.Engine.Interfaces.Managers;

public interface ISystemManager
{
    void AddRenderSystem(IRenderSystem renderSystem);

    void AddUpdateSystem(IUpdateSystem updateSystem);
}
