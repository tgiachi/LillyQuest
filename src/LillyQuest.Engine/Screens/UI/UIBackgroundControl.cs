using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Engine.Screens.UI;

public sealed class UIBackgroundControl : UIScreenControl
{
    public LyColor Color { get; set; } = LyColor.Black;
    public float Alpha { get; set; } = 0.5f;

    public UIBackgroundControl()
    {
        IsFocusable = false;
        IsEnabled = true;
    }

    public LyColor GetColorWithAlpha()
    {
        var alpha = (byte)Math.Clamp(MathF.Round(Color.A * Alpha), 0f, 255f);

        return Color.WithAlpha(alpha);
    }

    public override bool HandleMouseDown(Vector2 point)
        => false;

    public override bool HandleMouseMove(Vector2 point)
        => false;

    public override bool HandleMouseUp(Vector2 point)
        => false;

    public override void Render(SpriteBatch? spriteBatch, EngineRenderContext? renderContext)
    {
        if (spriteBatch == null || renderContext == null)
        {
            return;
        }

        spriteBatch.DrawRectangle(GetWorldPosition(), Size, GetColorWithAlpha());
    }
}
