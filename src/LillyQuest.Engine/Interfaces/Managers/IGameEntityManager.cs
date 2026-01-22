using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Engine.Interfaces.Managers;

/// <summary>
/// Manages the creation, addition, removal, and querying of game entities.
/// Maintains ordered list of entities and supports hierarchical parent-child relationships.
/// </summary>
public interface IGameEntityManager
{
    /// <summary>
    /// Gets all entities in the manager, ordered by their Order property and hierarchical depth.
    /// </summary>
    IReadOnlyList<IGameEntity> OrderedEntities { get; }

    /// <summary>
    /// Adds an entity to the manager as a root entity (no parent).
    /// </summary>
    /// <param name="entity">The entity to add</param>
    void AddEntity(IGameEntity entity);

    /// <summary>
    /// Adds an entity to the manager as a child of the specified parent entity.
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <param name="parent">The parent entity</param>
    void AddEntity(IGameEntity entity, IGameEntity parent);

    /// <summary>
    /// Creates a new entity of the specified type and adds it to the manager.
    /// The entity is resolved via Dependency Injection (DryIoc container).
    /// All required constructor dependencies must be registered in the container.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to create</typeparam>
    /// <returns>The newly created entity instance resolved from the DI container</returns>
    TEntity CreateEntity<TEntity>() where TEntity : IGameEntity;

    /// <summary>
    /// Retrieves an entity by its unique ID.
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>The entity if found, null otherwise</returns>
    IGameEntity? GetEntityById(uint id);

    /// <summary>
    /// Queries all entities that implement a specific interface (e.g., IUpdateableEntity, IRenderableEntity).
    /// Uses reflection-based filtering with caching for performance.
    /// </summary>
    /// <typeparam name="TInterface">The interface type to query for</typeparam>
    /// <returns>Read-only list of entities implementing the interface</returns>
    IReadOnlyList<TInterface> GetQueryOf<TInterface>() where TInterface : class;

    /// <summary>
    /// Removes an entity from the manager.
    /// </summary>
    /// <param name="entity">The entity to remove</param>
    void RemoveEntity(IGameEntity entity);
}
