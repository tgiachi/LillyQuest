using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Engine.Interfaces.Services;

public interface IParticlePixelRenderer
{
    void Render(SpriteBatch spriteBatch, TilesetSurfaceScreen screen, int layerIndex);
}
