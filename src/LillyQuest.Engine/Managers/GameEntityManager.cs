using LillyQuest.Engine.Interfaces.Components;
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Managers;

namespace LillyQuest.Engine.Managers;

/// <summary>
/// Manages the lifecycle and querying of game entities.
/// Maintains a global index of components by type for efficient type-based queries.
/// </summary>
public class GameEntityManager : IGameEntityManager
{
    private readonly List<IGameEntity> _entities = [];
    private readonly Dictionary<Type, List<IGameComponent>> _globalTypeIndex = [];

    /// <summary>
    /// Adds an entity to the manager and indexes all its components.
    /// </summary>
    public void AddEntity(IGameEntity entity)
    {
        _entities.Add(entity);
        IndexEntity(entity);
    }

    /// <summary>
    /// Removes an entity from the manager and deindexes all its components.
    /// </summary>
    public void RemoveEntity(IGameEntity entity)
    {
        _entities.Remove(entity);
        DeindexEntity(entity);
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
    /// </summary>
    private void IndexEntity(IGameEntity entity)
    {
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
