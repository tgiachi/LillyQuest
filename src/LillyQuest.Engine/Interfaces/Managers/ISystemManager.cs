using LillyQuest.Engine.Interfaces.Systems;

namespace LillyQuest.Engine.Interfaces.Managers;

public interface ISystemManager
{
    void AddRenderSystem(IRenderSystem renderSystem);

    void AddSystem<T>(T system) where T : ISystem;

    void AddUpdateSystem(IUpdateSystem updateSystem);
}
