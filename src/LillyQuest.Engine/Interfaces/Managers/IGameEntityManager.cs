using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Engine.Interfaces.Managers;

public interface IGameEntityManager
{
    void AddEntity(IGameEntity entity);
    void AddEntity(IGameEntity entity, IGameEntity parent);
    void RemoveEntity(IGameEntity entity);
    IGameEntity? GetEntityById(uint id);
    IReadOnlyList<TInterface> GetQueryOf<TInterface>() where TInterface : class;

    IReadOnlyList<IGameEntity> OrderedEntities { get; }
}
