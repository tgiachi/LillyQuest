using LillyQuest.Engine.Interfaces.Components;
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Managers;

namespace LillyQuest.Engine.Managers;

/// <summary>
/// Manages the lifecycle and querying of game entities.
/// Maintains a global index of components by type, sorted by entity Order.
/// Fires lifecycle events on entity add/remove.
/// </summary>
public class GameEntityManager : IGameEntityManager
{
    private readonly List<IGameEntity> _entities = [];
    private readonly Dictionary<Type, List<IGameComponent>> _globalTypeIndex = [];

    /// <summary>
    /// Fired when an entity is added to the manager.
    /// Event fires after the entity is fully indexed and sorted.
    /// </summary>
    public event GameEntityLifecycleHandler? OnGameEntityAdded;

    /// <summary>
    /// Fired when an entity is removed from the manager.
    /// Event fires after the entity is fully deindexed.
    /// </summary>
    public event GameEntityLifecycleHandler? OnGameEntityRemoved;

    /// <summary>
    /// Adds an entity to the manager and indexes all its components.
    /// Components are indexed and sorted by entity Order.
    /// OnGameEntityAdded event is fired after indexing completes.
    /// </summary>
    public void AddEntity(IGameEntity entity)
    {
        _entities.Add(entity);
        IndexEntity(entity);
        OnGameEntityAdded?.Invoke(entity);
    }

    /// <summary>
    /// Removes an entity from the manager and deindexes all its components.
    /// OnGameEntityRemoved event is fired after deindexing completes.
    /// </summary>
    public void RemoveEntity(IGameEntity entity)
    {
        _entities.Remove(entity);
        DeindexEntity(entity);
        OnGameEntityRemoved?.Invoke(entity);
    }

    /// <summary>
    /// Gets all components of a specific type across all entities.
    /// </summary>
    /// <typeparam name="TGameComponent">The type of component to retrieve.</typeparam>
    /// <returns>A collection of components of the specified type.</returns>
    public IEnumerable<TGameComponent> GetAllComponentsOfType<TGameComponent>()
        where TGameComponent : IGameComponent
    {
        var componentType = typeof(TGameComponent);
        if (_globalTypeIndex.TryGetValue(componentType, out var components))
        {
            return components.Cast<TGameComponent>();
        }
        return [];
    }

    /// <summary>
    /// Indexes all components of an entity in the global type index.
    /// After indexing, sorts all type lists by entity Order to maintain ordering.
    /// </summary>
    private void IndexEntity(IGameEntity entity)
    {
        // Add components to their type lists
        foreach (var component in entity.Components)
        {
            var componentType = component.GetType();
            if (!_globalTypeIndex.TryGetValue(componentType, out var components))
            {
                components = [];
                _globalTypeIndex[componentType] = components;
            }
            components.Add(component);
        }

        // Sort all affected type lists by entity Order
        foreach (var component in entity.Components)
        {
            var componentType = component.GetType();
            var typeList = _globalTypeIndex[componentType];
            typeList.Sort((a, b) => a.Owner!.Order.CompareTo(b.Owner!.Order));
        }
    }

    /// <summary>
    /// Deindexes all components of an entity from the global type index.
    /// </summary>
    private void DeindexEntity(IGameEntity entity)
    {
        foreach (var component in entity.Components)
        {
            var componentType = component.GetType();
            if (_globalTypeIndex.TryGetValue(componentType, out var components))
            {
                components.Remove(component);
                // Remove empty lists to save memory
                if (components.Count == 0)
                {
                    _globalTypeIndex.Remove(componentType);
                }
            }
        }
    }
}
