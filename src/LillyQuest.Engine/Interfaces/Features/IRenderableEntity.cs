using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;

namespace LillyQuest.Engine.Interfaces.Features;

/// <summary>
/// Marks an entity that can render itself.
/// </summary>
public interface IRenderableEntity : IEntityFeature
{
    /// <summary>
    /// Renders the entity for the current frame.
    /// </summary>
    void Render(SpriteBatch spriteBatch, EngineRenderContext context);
}
