using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Engine.Interfaces.Managers;

public interface IGameEntityManager
{
    IReadOnlyList<IGameEntity> OrderedEntities { get; }
    void AddEntity(IGameEntity entity);
    void AddEntity(IGameEntity entity, IGameEntity parent);
    IGameEntity? GetEntityById(uint id);
    IReadOnlyList<TInterface> GetQueryOf<TInterface>() where TInterface : class;
    void RemoveEntity(IGameEntity entity);
}
