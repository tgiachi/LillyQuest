using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Features;

namespace LillyQuest.Engine.Entities.Debug;

/// <summary>
/// Debug entity that displays a label in the bottom-left corner of the screen.
/// Shows "DEBUG MODE" indicator for development builds.
/// </summary>
public class DebugLabelGameObject : GameEntity, IRenderableEntity
{
    private readonly EngineRenderContext _renderContext;

    public DebugLabelGameObject(EngineRenderContext renderContext)
    {
        _renderContext = renderContext;
        IsActive = true;
        Name = "Debug Label";
    }

    public void Render(SpriteBatch spriteBatch, EngineRenderContext context)
    {
        // Position in bottom-left corner with padding
        var windowHeight = _renderContext.Window.Size.Y;
        var position = new Vector2(10, windowHeight - 30);

        spriteBatch.DrawText(new("default_font", 14, FontKind.TrueType), "DEBUG MODE", position, LyColor.Yellow);
    }
}
