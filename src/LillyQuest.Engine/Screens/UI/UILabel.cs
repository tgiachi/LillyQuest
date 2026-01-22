using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Engine.Screens.UI;

/// <summary>
/// Simple text label control.
/// </summary>
public sealed class UILabel : UIScreenControl
{
    public string Text { get; set; } = string.Empty;
    public string FontName { get; set; } = "default_font";
    public int FontSize { get; set; } = 14;
    public LyColor Color { get; set; } = LyColor.White;

    public override void Render(SpriteBatch? spriteBatch, EngineRenderContext? renderContext)
    {
        if (spriteBatch == null || renderContext == null || string.IsNullOrEmpty(Text))
        {
            return;
        }

        spriteBatch.DrawFont(FontName, FontSize, Text, GetWorldPosition(), Color);
    }
}
