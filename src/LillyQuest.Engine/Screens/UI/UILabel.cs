using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Engine.Screens.UI;

/// <summary>
/// Simple text label control.
/// </summary>
public sealed class UILabel : UIScreenControl
{
    public string Text { get; set; } = string.Empty;
    public FontRef Font { get; set; } = new("default_font", 14, FontKind.TrueType);
    public LyColor Color { get; set; } = LyColor.White;

    public override void Render(SpriteBatch? spriteBatch, EngineRenderContext? renderContext)
    {
        if (spriteBatch == null || renderContext == null || string.IsNullOrEmpty(Text))
        {
            return;
        }

        spriteBatch.DrawText(Font, Text, GetWorldPosition(), Color);
    }
}
