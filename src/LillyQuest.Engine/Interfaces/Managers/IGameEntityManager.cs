using LillyQuest.Engine.Interfaces.Components;
using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Engine.Interfaces.Managers;

public interface IGameEntityManager
{
    void AddEntity(IGameEntity entity);

    void RemoveEntity(IGameEntity entity);

    IEnumerable<TGameComponent> GetAllComponentsOfType<TGameComponent>() where TGameComponent : IGameComponent;
}
