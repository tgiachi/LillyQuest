using LillyQuest.Engine.Interfaces.GameObjects.Features;

namespace LillyQuest.Engine.Interfaces.Entities;

/// <summary>
/// Represents a game object that can contain multiple features and have a hierarchical structure.
/// Game objects are the primary objects in the game world and serve as containers for features.
/// </summary>
public interface IGameEntity
{
    /// <summary>
    /// Gets the unique identifier of this game object.
    /// </summary>
    uint Id { get; set; }

    /// <summary>
    /// Gets or sets the name of this game object.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets or sets the parent game object in the hierarchy, or null if this is a root object.
    /// </summary>
    IGameEntity? Parent { get; set; }

    /// <summary>
    /// Gets the collection of child game objects in the hierarchy.
    /// </summary>
    IEnumerable<IGameEntity> Children { get; }

    /// <summary>
    /// Gets or sets the rendering order (depth/z-order) of this game object.
    /// Higher values are rendered on top of lower values. Can be changed dynamically.
    /// </summary>
    int Order { get; set; }

    /// <summary>
    /// Gets the collection of features attached to this game object.
    /// </summary>
    IEnumerable<IGameObjectFeature> Features { get; }

    /// <summary>
    /// Gets a feature of the specified type attached to this game object.
    /// </summary>
    /// <typeparam name="T">The type of feature to retrieve</typeparam>
    /// <returns>The feature of type T, or null if not found</returns>
    T? GetFeature<T>() where T : class, IGameObjectFeature;

    /// <summary>
    /// Checks if this game object has a feature of the specified type.
    /// O(1) lookup time.
    /// </summary>
    /// <typeparam name="T">The type of feature to check</typeparam>
    /// <returns>true if the game object has a feature of type T, false otherwise</returns>
    bool HasFeature<T>() where T : class, IGameObjectFeature;

    /// <summary>
    /// Called when this game object is initialized.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Called when this game object is shut down.
    /// </summary>
    void Shutdown();

    /// <summary>
    /// Attempts to get a feature of the specified type without throwing an exception.
    /// O(1) lookup time.
    /// </summary>
    /// <typeparam name="T">The type of feature to retrieve</typeparam>
    /// <param name="feature">The feature if found, null otherwise</param>
    /// <returns>true if the feature was found, false otherwise</returns>
    bool TryGetFeature<T>(out T? feature) where T : class, IGameObjectFeature;
}
