using LillyQuest.Engine.Interfaces.Components;

namespace LillyQuest.Engine.Collections;

/// <summary>
/// High-performance collection for storing game components with O(1) type-based lookup.
/// Supports efficient per-entity component storage and global type-based queries.
/// </summary>
public class GameComponentCollection : IEnumerable<IGameComponent>
{
    private readonly List<IGameComponent> _components = [];
    private readonly Dictionary<Type, IGameComponent> _typeIndex = [];

    /// <summary>
    /// Adds a component to the collection.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if a component of the same type already exists.</exception>
    public void Add(IGameComponent component)
    {
        var componentType = component.GetType();
        if (_typeIndex.ContainsKey(componentType))
        {
            throw new InvalidOperationException(
                $"A component of type '{componentType.Name}' is already present in this collection. " +
                $"Each entity can have at most one component of each type.");
        }

        _components.Add(component);
        _typeIndex[componentType] = component;
    }

    /// <summary>
    /// Retrieves a component of the specified type, or null if not found.
    /// O(1) lookup time.
    /// </summary>
    public TComponent? GetComponent<TComponent>() where TComponent : class, IGameComponent
    {
        var componentType = typeof(TComponent);
        return _typeIndex.TryGetValue(componentType, out var component)
            ? component as TComponent
            : null;
    }

    /// <summary>
    /// Enumerates all components in the collection.
    /// </summary>
    public IEnumerator<IGameComponent> GetEnumerator() => _components.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Checks if a component of the specified type exists in the collection.
    /// O(1) lookup time.
    /// </summary>
    public bool HasComponent<TComponent>() where TComponent : class, IGameComponent
    {
        return _typeIndex.ContainsKey(typeof(TComponent));
    }

    /// <summary>
    /// Removes a component from the collection.
    /// Returns true if the component was found and removed, false otherwise.
    /// </summary>
    public bool Remove(IGameComponent component)
    {
        var componentType = component.GetType();
        var removed = _components.Remove(component);
        if (removed)
        {
            _typeIndex.Remove(componentType);
        }
        return removed;
    }

    /// <summary>
    /// Removes all components from the collection.
    /// </summary>
    public void Clear()
    {
        _components.Clear();
        _typeIndex.Clear();
    }

    /// <summary>
    /// Gets the number of components in the collection.
    /// </summary>
    public int Count => _components.Count;
}
