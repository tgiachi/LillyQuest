using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.GameObjects.Features;
using LillyQuest.Engine.Interfaces.Managers;

namespace LillyQuest.Engine.Managers;

/// <summary>
/// Manages the lifecycle and querying of game objects and their features.
/// Maintains a global index of features by type, sorted by entity Order.
/// Fires lifecycle events on object add/remove.
/// </summary>
public class GameEntityManager : IGameEntityManager
{
    private readonly List<IGameEntity> _entities = [];
    private readonly Dictionary<Type, List<IGameObjectFeature>> _globalTypeIndex = [];

    /// <summary>
    /// Fired when a game object is added to the manager.
    /// Event fires after the object is fully indexed and sorted.
    /// </summary>
    public event GameEntityLifecycleHandler? OnGameEntityAdded;

    /// <summary>
    /// Fired when a game object is removed from the manager.
    /// Event fires after the object is fully deindexed.
    /// </summary>
    public event GameEntityLifecycleHandler? OnGameEntityRemoved;

    /// <summary>
    /// Adds a game object to the manager and indexes all its features.
    /// Features are indexed and sorted by entity Order.
    /// OnGameEntityAdded event is fired after indexing completes.
    /// </summary>
    public void AddEntity(IGameEntity entity)
    {
        _entities.Add(entity);
        entity.Initialize();
        IndexEntity(entity);
        OnGameEntityAdded?.Invoke(entity);
    }

    /// <summary>
    /// Removes a game object from the manager and deindexes all its features.
    /// OnGameEntityRemoved event is fired after deindexing completes.
    /// </summary>
    public void RemoveEntity(IGameEntity entity)
    {
        _entities.Remove(entity);
        DeindexEntity(entity);
        entity.Shutdown();
        OnGameEntityRemoved?.Invoke(entity);
    }

    /// <summary>
    /// Queries all features of a specific type across all game objects.
    /// </summary>
    /// <typeparam name="TFeature">The type of feature to query.</typeparam>
    /// <returns>A lazy sequence of features matching the specified type.</returns>
    public IEnumerable<TFeature> QueryOfType<TFeature>() where TFeature : IGameObjectFeature
    {
        var featureType = typeof(TFeature);

        if (_globalTypeIndex.TryGetValue(featureType, out var features))
        {
            return features.Cast<TFeature>();
        }

        return [];
    }

    /// <summary>
    /// Indexes all features of a game object in the global type index.
    /// After indexing, sorts all type lists by entity Order to maintain ordering.
    /// </summary>
    private void IndexEntity(IGameEntity entity)
    {
        // Add features to their type lists
        foreach (var feature in entity.Features)
        {
            var featureType = feature.GetType();

            if (!_globalTypeIndex.TryGetValue(featureType, out var features))
            {
                features = [];
                _globalTypeIndex[featureType] = features;
            }
            features.Add(feature);
        }

        // Sort all affected type lists by entity Order
        foreach (var feature in entity.Features)
        {
            var featureType = feature.GetType();
            var typeList = _globalTypeIndex[featureType];
            typeList.Sort(
                (a, b) =>
                {
                    // Find the entities that own these features
                    var aEntity = _entities.FirstOrDefault(e => e.Features.Contains(a));
                    var bEntity = _entities.FirstOrDefault(e => e.Features.Contains(b));

                    return aEntity?.Order.CompareTo(bEntity?.Order) ?? 0;
                }
            );
        }
    }

    /// <summary>
    /// Deindexes all features of a game object from the global type index.
    /// </summary>
    private void DeindexEntity(IGameEntity entity)
    {
        foreach (var feature in entity.Features)
        {
            var featureType = feature.GetType();

            if (_globalTypeIndex.TryGetValue(featureType, out var features))
            {
                features.Remove(feature);

                // Remove empty lists to save memory
                if (features.Count == 0)
                {
                    _globalTypeIndex.Remove(featureType);
                }
            }
        }
    }
}
