using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
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
    private UIBackgroundControl? _modalBackground;

    public LyColor ModalBackgroundColor { get; set; } = LyColor.Black;
    public float ModalBackgroundAlpha { get; set; } = 0.5f;

    public override bool OnMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons)
    {
        var modal = GetTopmostModal();
        EnsureModalBackground(modal);

        if (modal != null)
        {
            var bounds = modal.GetBounds();
            var point = new Vector2(x, y);

            if (point.X < bounds.Origin.X ||
                point.X > bounds.Origin.X + bounds.Size.X ||
                point.Y < bounds.Origin.Y ||
                point.Y > bounds.Origin.Y + bounds.Size.Y)
            {
                return false;
            }

            if (modal.HandleMouseDown(point))
            {
                _activeControl = modal;
                if (modal.IsFocusable)
                {
                    Root.FocusManager.RequestFocus(modal);
                }
                Root.BringToFront(modal);

                return true;
            }

            return false;
        }

        var hit = Root.HitTest(new(x, y));

        if (hit == null)
        {
            return false;
        }

        if (hit.HandleMouseDown(new(x, y)))
        {
            _activeControl = hit;
            if (hit.IsFocusable)
            {
                Root.FocusManager.RequestFocus(hit);
            }
            Root.BringToFront(hit);

            return true;
        }

        return false;
    }

    public override bool OnMouseMove(int x, int y)
        => _activeControl?.HandleMouseMove(new(x, y)) ?? false;

    public override bool OnMouseUp(int x, int y, IReadOnlyList<MouseButton> buttons)
    {
        if (_activeControl == null)
        {
            return false;
        }

        var handled = _activeControl.HandleMouseUp(new(x, y));
        _activeControl = null;

        return handled;
    }

    private UIWindow? GetTopmostModal()
    {
        return Root.Children
                   .OfType<UIWindow>()
                   .Where(window => window.IsModal)
                   .OrderByDescending(window => window.ZIndex)
                   .ThenByDescending(window => Root.Children.ToList().IndexOf(window))
                   .FirstOrDefault();
    }

    private void EnsureModalBackground(UIWindow? modal)
    {
        if (modal == null)
        {
            if (_modalBackground != null)
            {
                Root.Remove(_modalBackground);
                _modalBackground = null;
            }

            return;
        }

        if (_modalBackground == null)
        {
            _modalBackground = new UIBackgroundControl();
            Root.Add(_modalBackground);
        }

        _modalBackground.Position = Vector2.Zero;
        _modalBackground.Size = Size;
        _modalBackground.Color = ModalBackgroundColor;
        _modalBackground.Alpha = ModalBackgroundAlpha;
        _modalBackground.ZIndex = modal.ZIndex - 1;
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
