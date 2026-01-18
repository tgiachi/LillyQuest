using LillyQuest.Engine.Collections;
using LillyQuest.Engine.Interfaces.Components;
using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Engine.Entities;

/// <summary>
/// Concrete implementation of IGameEntity.
/// Represents a game entity that can contain components and has rendering order properties.
/// </summary>
public class GameEntity : IGameEntity
{
    private readonly GameComponentCollection _components = [];

    /// <summary>
    /// Gets the unique identifier of this entity.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// Gets the name of this entity.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this entity is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the rendering order (depth/z-order) of this entity.
    /// </summary>
    public uint Order { get; set; }

    /// <summary>
    /// Gets the collection of components attached to this entity.
    /// </summary>
    public IEnumerable<IGameComponent> Components => _components;

    /// <summary>
    /// Initializes a new instance of the GameEntity class.
    /// </summary>
    /// <param name="id">The unique identifier for this entity.</param>
    /// <param name="name">The name of this entity.</param>
    public GameEntity(uint id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Adds a component to this entity.
    /// </summary>
    /// <param name="component">The component to add.</param>
    /// <exception cref="InvalidOperationException">Thrown if a component of the same type already exists.</exception>
    public void AddComponent(IGameComponent component)
    {
        component.Owner = this;
        _components.Add(component);
        component.Initialize();
    }

    /// <summary>
    /// Retrieves a component of the specified type, or null if not found.
    /// O(1) lookup time.
    /// </summary>
    public TComponent? GetComponent<TComponent>() where TComponent : class, IGameComponent
    {
        return _components.GetComponent<TComponent>();
    }

    /// <summary>
    /// Checks if a component of the specified type exists in this entity.
    /// O(1) lookup time.
    /// </summary>
    public bool HasComponent<TComponent>() where TComponent : class, IGameComponent
    {
        return _components.HasComponent<TComponent>();
    }

    /// <summary>
    /// Removes a component from this entity.
    /// Returns true if the component was found and removed, false otherwise.
    /// </summary>
    public bool RemoveComponent(IGameComponent component)
    {
        return _components.Remove(component);
    }
}
