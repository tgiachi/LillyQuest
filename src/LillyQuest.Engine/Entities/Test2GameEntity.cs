using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Core.Graphics.Text;

namespace LillyQuest.Engine.Entities;

public class Test2GameEntity : GameEntity, IRenderableEntity
{
    public Test2GameEntity()
        => Name = "Test2";

    public void Render(SpriteBatch spriteBatch, EngineRenderContext context)
    {
        spriteBatch.DrawText(new FontRef("default_font", 14, FontKind.TrueType), "Test2 Entity Rendered", new(100, 100), LyColor.Red);
    }
}
