using LillyQuest.Engine.Collections;
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.GameObjects.Features;

namespace LillyQuest.Engine.Entities;

/// <summary>
/// Concrete implementation of IGameEntity.
/// Represents a game object that can contain features, has a hierarchical structure, and supports lifecycle events.
/// </summary>
public class GameEntity : IGameEntity
{
    private readonly GameFeatureCollection _features = [];
    private readonly List<IGameEntity> _children = [];
    private IGameEntity? _parent;

    /// <summary>
    /// Gets or sets the unique identifier of this game object.
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// Gets or sets the name of this game object.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the parent game object in the hierarchy.
    /// </summary>
    public IGameEntity? Parent
    {
        get => _parent;
        set
        {
            if (_parent != null)
            {
                // Remove from old parent
                var oldChildren = (_parent as GameEntity)?._children;
                oldChildren?.Remove(this);
            }

            _parent = value;

            if (_parent != null)
            {
                // Add to new parent
                var newChildren = (_parent as GameEntity)?._children;
                if (newChildren != null && !newChildren.Contains(this))
                {
                    newChildren.Add(this);
                }
            }
        }
    }

    /// <summary>
    /// Gets the collection of child game objects.
    /// </summary>
    public IEnumerable<IGameEntity> Children => _children.AsReadOnly();

    /// <summary>
    /// Gets or sets the rendering order (depth/z-order) of this game object.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets the collection of features attached to this game object.
    /// </summary>
    public IEnumerable<IGameObjectFeature> Features => _features;

    /// <summary>
    /// Initializes a new instance of the GameEntity class.
    /// </summary>
    /// <param name="id">The unique identifier for this game object.</param>
    /// <param name="name">The name of this game object.</param>
    public GameEntity(uint id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Called when this game object is initialized.
    /// </summary>
    public virtual void Initialize()
    {
        foreach (var feature in _features)
        {
            if (feature is IGameObjectFeature f)
            {
                // Features may have their own initialization logic
            }
        }
    }

    /// <summary>
    /// Called when this game object is shut down.
    /// </summary>
    public virtual void Shutdown()
    {
        // Clean up children
        foreach (var child in _children)
        {
            child.Shutdown();
        }
        _children.Clear();

        // Dispose features
        foreach (var feature in _features)
        {
            if (feature is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    /// <summary>
    /// Adds a feature to this game object.
    /// </summary>
    /// <param name="feature">The feature to add.</param>
    public void AddFeature(IGameObjectFeature feature)
    {
        _features.Add(feature);
    }

    /// <summary>
    /// Gets a feature of the specified type, or null if not found.
    /// O(1) lookup time.
    /// </summary>
    public TFeature? GetFeature<TFeature>() where TFeature : class, IGameObjectFeature
    {
        return _features.GetFeature<TFeature>();
    }

    /// <summary>
    /// Checks if a feature of the specified type exists in this game object.
    /// O(1) lookup time.
    /// </summary>
    public bool HasFeature<TFeature>() where TFeature : class, IGameObjectFeature
    {
        return _features.HasFeature<TFeature>();
    }

    /// <summary>
    /// Attempts to get a feature of the specified type without throwing an exception.
    /// O(1) lookup time.
    /// </summary>
    public bool TryGetFeature<TFeature>(out TFeature? feature) where TFeature : class, IGameObjectFeature
    {
        feature = GetFeature<TFeature>();
        return feature != null;
    }

    /// <summary>
    /// Removes a feature from this game object.
    /// </summary>
    public bool RemoveFeature(IGameObjectFeature feature)
    {
        return _features.Remove(feature);
    }
}
