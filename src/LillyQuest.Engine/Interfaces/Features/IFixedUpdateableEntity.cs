using LillyQuest.Core.Primitives;

namespace LillyQuest.Engine.Interfaces.Features;

/// <summary>
/// Marks an entity that updates on a fixed timestep.
/// </summary>
public interface IFixedUpdateableEntity : IEntityFeature
{
    /// <summary>
    /// Updates the entity on the fixed timestep.
    /// </summary>
    void FixedUpdate(GameTime gameTime);
}
