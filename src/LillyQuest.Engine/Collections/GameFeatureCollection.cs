using LillyQuest.Engine.Interfaces.GameObjects.Features;

namespace LillyQuest.Engine.Collections;

/// <summary>
/// High-performance collection for storing game features with O(1) type-based lookup.
/// Supports efficient per-entity feature storage and global type-based queries.
/// </summary>
public class GameFeatureCollection : IEnumerable<IGameObjectFeature>
{
    private readonly List<IGameObjectFeature> _features = [];
    private readonly Dictionary<Type, IGameObjectFeature> _typeIndex = [];

    /// <summary>
    /// Adds a feature to the collection.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if a feature of the same type already exists.</exception>
    public void Add(IGameObjectFeature feature)
    {
        var featureType = feature.GetType();
        if (_typeIndex.ContainsKey(featureType))
        {
            throw new InvalidOperationException(
                $"A feature of type '{featureType.Name}' is already present in this collection. " +
                $"Each entity can have at most one feature of each type.");
        }

        _features.Add(feature);
        _typeIndex[featureType] = feature;
    }

    /// <summary>
    /// Retrieves a feature of the specified type, or null if not found.
    /// O(1) lookup time.
    /// </summary>
    public TFeature? GetFeature<TFeature>() where TFeature : class, IGameObjectFeature
    {
        var featureType = typeof(TFeature);
        return _typeIndex.TryGetValue(featureType, out var feature)
            ? feature as TFeature
            : null;
    }

    /// <summary>
    /// Enumerates all features in the collection.
    /// </summary>
    public IEnumerator<IGameObjectFeature> GetEnumerator() => _features.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Checks if a feature of the specified type exists in the collection.
    /// O(1) lookup time.
    /// </summary>
    public bool HasFeature<TFeature>() where TFeature : class, IGameObjectFeature
    {
        return _typeIndex.ContainsKey(typeof(TFeature));
    }

    /// <summary>
    /// Removes a feature from the collection.
    /// Returns true if the feature was found and removed, false otherwise.
    /// </summary>
    public bool Remove(IGameObjectFeature feature)
    {
        var featureType = feature.GetType();
        var removed = _features.Remove(feature);
        if (removed)
        {
            _typeIndex.Remove(featureType);
        }
        return removed;
    }

    /// <summary>
    /// Removes all features from the collection.
    /// </summary>
    public void Clear()
    {
        _features.Clear();
        _typeIndex.Clear();
    }

    /// <summary>
    /// Gets the number of features in the collection.
    /// </summary>
    public int Count => _features.Count;
}
