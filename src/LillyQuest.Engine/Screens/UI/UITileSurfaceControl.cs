using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Engine.Screens.UI;

/// <summary>
/// UI control backed by a tile surface screen.
/// </summary>
public sealed class UITileSurfaceControl : UIScreenControl
{
    public TilesetSurfaceScreen Surface { get; }

    public UITileSurfaceControl(ITilesetManager tilesetManager, int width, int height)
    {
        Surface = new(tilesetManager)
        {
            LayerCount = 1,
            TileViewSize = new(width, height)
        };
        Surface.InitializeLayers(1);
    }

    public override void Render(SpriteBatch? spriteBatch, EngineRenderContext? renderContext)
    {
        if (spriteBatch == null || renderContext == null)
        {
            return;
        }

        Surface.Position = GetWorldPosition();
        Surface.Render(spriteBatch, renderContext);
    }
}
