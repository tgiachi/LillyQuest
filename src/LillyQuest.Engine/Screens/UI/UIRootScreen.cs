using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Engine.Managers.Screens.Base;
using Silk.NET.Input;

namespace LillyQuest.Engine.Screens.UI;

/// <summary>
/// Root UI screen that hosts UI controls and handles input dispatch.
/// </summary>
public sealed class UIRootScreen : BaseScreen
{
    public UIScreenRoot Root { get; } = new();
    private UIScreenControl? _activeControl;

    public override bool OnMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons)
    {
        var hit = Root.HitTest(new Vector2(x, y));
        if (hit == null)
        {
            return false;
        }

        if (hit.HandleMouseDown(new Vector2(x, y)))
        {
            _activeControl = hit;
            return true;
        }

        return false;
    }

    public override bool OnMouseMove(int x, int y)
        => _activeControl?.HandleMouseMove(new Vector2(x, y)) ?? false;

    public override bool OnMouseUp(int x, int y, IReadOnlyList<MouseButton> buttons)
    {
        if (_activeControl == null)
        {
            return false;
        }

        var handled = _activeControl.HandleMouseUp(new Vector2(x, y));
        _activeControl = null;
        return handled;
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
