using LillyQuest.Core.Extensions.Strings;
using LillyQuest.Engine.Collections;
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Managers;
using Serilog;

namespace LillyQuest.Engine.Managers.Entities;

public sealed class GameEntityManager : IGameEntityManager
{
    private readonly ILogger _logger = Log.ForContext<GameEntityManager>();
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



        if (string.IsNullOrEmpty(entity.Name))
        {
            entity.Name = $"{entity.GetType().Name}_{entity.Id}".ToSnakeCase();

            if (entity.Parent is not null)
            {
                entity.Name = $"{entity.Parent.Name}_{entity.Name}_{entity.Id}".ToSnakeCase();
            }
        }
        else
        {
            entity.Name = entity.Name.ToSnakeCase();
        }

        _logger.Debug(
            "Assigned ID {EntityId} to entity {EntityName} (Parent: {Parent})",
            entity.Id,
            entity.Name,
            entity.Parent?.Name ?? "null"
        );


        _entitiesById[entity.Id] = entity;

        if (entity.Children is not null)
        {
            foreach (var child in entity.Children)
            {
                AssignIdsRecursively(child);
            }
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
            _logger.Debug(
                "Removed entity ID {EntityId} {EntityName} (Parent: {Parent})",
                entity.Id,
                entity.Name,
                entity.Parent?.Name ?? "null"
            );
        }
    }
}
