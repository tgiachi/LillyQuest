using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.GameObjects.Features;

namespace LillyQuest.Engine.Interfaces.Features;

/// <summary>
/// Interface for features that need to render using SpriteBatch.
/// Features are rendered in order by RenderOrder property.
/// </summary>
public interface IRenderFeature : IGameObjectFeature
{
    /// <summary>
    /// Rendering order for this feature. Lower values render first (background).
    /// Higher values render last (foreground/overlay).
    /// </summary>
    int RenderOrder { get; }

    /// <summary>
    /// Renders this feature to the SpriteBatch.
    /// </summary>
    void Render(SpriteBatch spriteBatch, GameTime gameTime);
}
