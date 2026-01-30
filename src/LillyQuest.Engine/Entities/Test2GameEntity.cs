using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Features;

namespace LillyQuest.Engine.Entities;

public class Test2GameEntity : GameEntity, IRenderableEntity
{
    public Test2GameEntity()
        => Name = "Test2";

    public void Render(SpriteBatch spriteBatch, EngineRenderContext context)
    {
        spriteBatch.DrawText(
            new("default_font", 14, FontKind.TrueType),
            "Test2 Entity Rendered",
            new(100, 100),
            LyColor.Red
        );
    }
}
