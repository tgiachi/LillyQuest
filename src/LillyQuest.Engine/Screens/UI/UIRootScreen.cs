using System;
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

            if (modal.HandleMouseDown(point, buttons))
            {
                _activeControl = modal;
                var focusTarget = ResolveFocusableAtPoint(modal, point) ?? (modal.IsFocusable ? modal : null);
                if (focusTarget != null)
                {
                    Root.FocusManager.RequestFocus(focusTarget);
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

        if (hit.HandleMouseDown(new(x, y), buttons))
        {
            _activeControl = hit;
            var focusTarget = ResolveFocusableAtPoint(hit, new(x, y)) ?? (hit.IsFocusable ? hit : null);
            if (focusTarget != null)
            {
                Root.FocusManager.RequestFocus(focusTarget);
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

        var handled = _activeControl.HandleMouseUp(new(x, y), buttons);
        _activeControl = null;

        return handled;
    }

    public override bool OnMouseWheel(int x, int y, float delta)
    {
        var modal = GetTopmostModal();
        if (modal != null)
        {
            return modal.HandleMouseWheel(new(x, y), delta);
        }

        var hit = Root.HitTest(new(x, y));
        return hit?.HandleMouseWheel(new(x, y), delta) ?? false;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        foreach (var control in Root.Children)
        {
            control.Update(gameTime);
        }
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

    private static UIScreenControl? ResolveFocusableAtPoint(UIScreenControl control, Vector2 point)
    {
        var children = GetChildren(control);

        if (children.Count > 0)
        {
            var childPoint = point;

            if (control is UIScrollContent scroll)
            {
                var viewport = scroll.GetViewportBounds();
                if (point.X < viewport.Origin.X ||
                    point.X > viewport.Origin.X + viewport.Size.X ||
                    point.Y < viewport.Origin.Y ||
                    point.Y > viewport.Origin.Y + viewport.Size.Y)
                {
                    return control.IsFocusable ? control : null;
                }

                childPoint = point + scroll.ScrollOffset;
            }

            var childList = children.ToList();
            foreach (var child in childList
                                  .OrderByDescending(c => c.ZIndex)
                                  .ThenByDescending(c => childList.IndexOf(c)))
            {
                var bounds = child.GetBounds();
                if (childPoint.X < bounds.Origin.X ||
                    childPoint.X > bounds.Origin.X + bounds.Size.X ||
                    childPoint.Y < bounds.Origin.Y ||
                    childPoint.Y > bounds.Origin.Y + bounds.Size.Y)
                {
                    continue;
                }

                var nested = ResolveFocusableAtPoint(child, childPoint);
                if (nested != null)
                {
                    return nested;
                }

                if (child.IsFocusable)
                {
                    return child;
                }
            }
        }

        return control.IsFocusable ? control : null;
    }

    private static IReadOnlyList<UIScreenControl> GetChildren(UIScreenControl control)
    {
        return control switch
        {
            UIWindow window => window.Children,
            UIScrollContent scroll => scroll.Children,
            UIButton button => button.Children,
            _ => Array.Empty<UIScreenControl>()
        };
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
