using LillyQuest.Engine.Collections;
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Managers;

namespace LillyQuest.Engine.Managers.Entities;

public sealed class GameEntityManager : IGameEntityManager
{
    private readonly GameEntityCollection _collection = new();
    private readonly Dictionary<uint, IGameEntity> _entitiesById = new();
    private uint _nextId = 1;

    public IReadOnlyList<IGameEntity> OrderedEntities => _collection.OrderedEntities;

    public void AddEntity(IGameEntity entity)
    {
        AddEntityInternal(entity, null);
    }

    public void AddEntity(IGameEntity entity, IGameEntity parent)
    {
        ArgumentNullException.ThrowIfNull(parent);
        AddEntityInternal(entity, parent);
    }

    public IGameEntity? GetEntityById(uint id)
        => _entitiesById.GetValueOrDefault(id);

    public IReadOnlyList<TInterface> GetQueryOf<TInterface>() where TInterface : class
        => _collection.GetQueryOf<TInterface>();

    public void RemoveEntity(IGameEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        RemoveSubtreeFromMap(entity);
        _collection.Remove(entity);
    }

    private void AddEntityInternal(IGameEntity entity, IGameEntity? parent)
    {
        ArgumentNullException.ThrowIfNull(entity);

        AssignIdsRecursively(entity);
        _collection.Add(entity, parent);
    }

    private void AssignIdsRecursively(IGameEntity entity)
    {
        if (entity.Id == 0)
        {
            entity.Id = _nextId++;
        }
        else if (_entitiesById.TryGetValue(entity.Id, out var existing) && !ReferenceEquals(existing, entity))
        {
            throw new InvalidOperationException($"Entity id {entity.Id} is already assigned.");
        }

        _entitiesById[entity.Id] = entity;

        if (entity.Children is null)
        {
            entity.Children = new List<IGameEntity>();
        }

        foreach (var child in entity.Children)
        {
            AssignIdsRecursively(child);
        }
    }

    private void RemoveSubtreeFromMap(IGameEntity entity)
    {
        if (entity.Children is not null)
        {
            foreach (var child in entity.Children)
            {
                RemoveSubtreeFromMap(child);
            }
        }

        if (entity.Id != 0)
        {
            _entitiesById.Remove(entity.Id);
        }
    }
}
