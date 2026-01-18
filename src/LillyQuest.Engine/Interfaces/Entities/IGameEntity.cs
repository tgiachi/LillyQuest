using LillyQuest.Engine.Interfaces.Components;

namespace LillyQuest.Engine.Interfaces.Entities;

/// <summary>
/// Represents a game entity that can contain multiple components.
/// Entities are the primary objects in the game world and serve as containers for components.
/// </summary>
public interface IGameEntity
{
    /// <summary>
    /// Gets the unique identifier of this entity.
    /// </summary>
    uint Id { get; }

    /// <summary>
    /// Gets the name of this entity.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this entity is currently active in the game world.
    /// </summary>
    bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the rendering order (depth/z-order) of this entity.
    /// Higher values are rendered on top of lower values. Can be changed dynamically.
    /// </summary>
    uint Order { get; set; }

    /// <summary>
    /// Gets the collection of components attached to this entity.
    /// </summary>
    IEnumerable<IGameComponent> Components { get; }
}
