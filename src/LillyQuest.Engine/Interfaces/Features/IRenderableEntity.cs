using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;

namespace LillyQuest.Engine.Interfaces.Features;

public interface IRenderableEntity
{
    void Render(SpriteBatch spriteBatch, EngineRenderContext context);
}
