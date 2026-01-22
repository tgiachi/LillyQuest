using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Engine.Managers.Screens.Base;
using Silk.NET.Input;

namespace LillyQuest.Engine.Screens.UI;

/// <summary>
/// Screen overlay that hosts UI controls.
/// </summary>
public sealed class UIScreenOverlay : BaseScreen
{
    public UIScreenRoot Root { get; } = new();

    public override bool HitTest(int x, int y)
        => true;

    public override bool OnMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons)
    {
        var hit = Root.HitTest(new(x, y));

        return hit != null && hit.HandleMouseDown(new(x, y));
    }

    public override void Render(SpriteBatch spriteBatch, EngineRenderContext renderContext)
    {
        foreach (var control in Root.Children.OrderBy(c => c.ZIndex))
        {
            if (!control.IsVisible)
            {
                continue;
            }

            control.Render(spriteBatch, renderContext);
        }
    }
}
