using LillyQuest.Engine.Interfaces.Components;
using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Engine.Interfaces.Managers;

/// <summary>
/// Manages the lifecycle and querying of game entities and their components.
/// Provides methods to add and remove entities, as well as query components by type.
/// </summary>
public interface IGameEntityManager
{
    /// <summary>
    /// Adds an entity to the manager.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    void AddEntity(IGameEntity entity);

    /// <summary>
    /// Removes an entity from the manager.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    void RemoveEntity(IGameEntity entity);

    /// <summary>
    /// Gets all components of a specific type that are attached to any entity.
    /// </summary>
    /// <typeparam name="TGameComponent">The type of component to retrieve.</typeparam>
    /// <returns>A collection of components of the specified type.</returns>
    IEnumerable<TGameComponent> GetAllComponentsOfType<TGameComponent>() where TGameComponent : IGameComponent;
}
