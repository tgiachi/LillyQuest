using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.GameObjects.Features;

namespace LillyQuest.Engine.Interfaces.Managers;

/// <summary>
/// Delegate for game object lifecycle events.
/// </summary>
/// <param name="entity">The game object that was added or removed.</param>
public delegate void GameEntityLifecycleHandler(IGameEntity entity);

/// <summary>
/// Manages the lifecycle and querying of game objects and their features.
/// Provides methods to add and remove objects, as well as query features by type.
/// </summary>
public interface IGameEntityManager
{
    /// <summary>
    /// Fired when a game object is added to the manager.
    /// Event fires after the object is fully indexed and sorted.
    /// </summary>
    event GameEntityLifecycleHandler? OnGameEntityAdded;

    /// <summary>
    /// Fired when a game object is removed from the manager.
    /// Event fires after the object is fully deindexed.
    /// </summary>
    event GameEntityLifecycleHandler? OnGameEntityRemoved;

    /// <summary>
    /// Creates a new game entity with the specified name.
    /// The entity is not added to the manager; use AddEntity() to add it.
    /// </summary>
    /// <param name="name">The name of the entity.</param>
    /// <returns>A new game entity instance.</returns>
    IGameEntity CreateEntity(string name);

    /// <summary>
    /// Adds a game object to the manager.
    /// </summary>
    /// <param name="entity">The game object to add.</param>
    void AddEntity(IGameEntity entity);

    /// <summary>
    /// Queries all features of a specific type across all game objects.
    /// </summary>
    /// <typeparam name="TFeature">The type of feature to query.</typeparam>
    /// <returns>A lazy sequence of features matching the specified type.</returns>
    IEnumerable<TFeature> QueryOfType<TFeature>() where TFeature : IGameObjectFeature;

    /// <summary>
    /// Removes a game object from the manager.
    /// </summary>
    /// <param name="entity">The game object to remove.</param>
    void RemoveEntity(IGameEntity entity);
}
