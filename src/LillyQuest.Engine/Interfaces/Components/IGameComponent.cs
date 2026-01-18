using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Engine.Interfaces.Components;

/// <summary>
/// Represents a component that can be attached to a game entity.
/// Components are used to add behavior and data to entities in an entity-component system.
/// </summary>
public interface IGameComponent
{
    /// <summary>
    /// Gets or sets the entity that owns this component.
    /// </summary>
    IGameEntity Owner { get; set; }

    /// <summary>
    /// Initializes the component.
    /// This method is called when the component is created and attached to an entity.
    /// </summary>
    void Initialize();
}
