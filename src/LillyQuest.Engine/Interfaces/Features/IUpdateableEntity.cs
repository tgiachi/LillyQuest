using LillyQuest.Core.Primitives;

namespace LillyQuest.Engine.Interfaces.Features;

/// <summary>
/// Marks an entity that can update each frame.
/// </summary>
public interface IUpdateableEntity : IEntityFeature
{
    /// <summary>
    /// Updates the entity for the current frame.
    /// </summary>
    void Update(GameTime gameTime);
}
