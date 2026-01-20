using LillyQuest.Engine.Data.Input;
using LillyQuest.Engine.Interfaces.Managers;
using Serilog;
using Silk.NET.Input;

namespace LillyQuest.Engine.Managers.Screens;

/// <summary>
/// Manages the screen hierarchy for input dispatch and rendering.
/// Implements hierarchical input consumption (modal screens block input to screens below).
/// </summary>
public sealed class ScreenManager : IScreenManager
{
    private readonly ILogger _logger = Log.ForContext<ScreenManager>();
    private IScreen? _rootScreen;

    public IScreen? RootScreen => _rootScreen;

    /// <summary>
    /// Sets the root screen of the hierarchy.
    /// </summary>
    public void SetRootScreen(IScreen? screen)
    {
        if (_rootScreen != screen)
        {
            _rootScreen?.OnHide();
            _rootScreen = screen;
            _rootScreen?.OnShow();
            _logger.Information("Set root screen to {ScreenId}", screen?.ConsumerId ?? "null");
        }
    }

    /// <summary>
    /// Hierarchical dispatch for key press events.
    /// Searches from top of hierarchy downward, stops at first consumer that handles input.
    /// </summary>
    public bool DispatchKeyPress(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        if (_rootScreen == null || !_rootScreen.IsActive)
            return false;

        return DispatchToHierarchy(_rootScreen, consumer => consumer.OnKeyPress(modifier, keys));
    }

    /// <summary>
    /// Hierarchical dispatch for key repeat events.
    /// </summary>
    public bool DispatchKeyRepeat(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        if (_rootScreen == null || !_rootScreen.IsActive)
            return false;

        return DispatchToHierarchy(_rootScreen, consumer => consumer.OnKeyRepeat(modifier, keys));
    }

    /// <summary>
    /// Hierarchical dispatch for key release events.
    /// </summary>
    public bool DispatchKeyRelease(KeyModifierType modifier, IReadOnlyList<Key> keys)
    {
        if (_rootScreen == null || !_rootScreen.IsActive)
            return false;

        return DispatchToHierarchy(_rootScreen, consumer => consumer.OnKeyRelease(modifier, keys));
    }

    /// <summary>
    /// Hierarchical dispatch for mouse move events.
    /// </summary>
    public bool DispatchMouseMove(int x, int y)
    {
        if (_rootScreen == null || !_rootScreen.IsActive)
            return false;

        return DispatchToHierarchy(_rootScreen, consumer => consumer.OnMouseMove(x, y));
    }

    /// <summary>
    /// Hierarchical dispatch for mouse down events.
    /// Uses hit-testing to find the topmost screen at the click position.
    /// </summary>
    public bool DispatchMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons)
    {
        if (_rootScreen == null || !_rootScreen.IsActive)
            return false;

        return DispatchToHierarchyWithHitTest(_rootScreen, x, y, consumer => consumer.OnMouseDown(x, y, buttons));
    }

    /// <summary>
    /// Hierarchical dispatch for mouse up events.
    /// </summary>
    public bool DispatchMouseUp(int x, int y, IReadOnlyList<MouseButton> buttons)
    {
        if (_rootScreen == null || !_rootScreen.IsActive)
            return false;

        return DispatchToHierarchy(_rootScreen, consumer => consumer.OnMouseUp(x, y, buttons));
    }

    /// <summary>
    /// Hierarchical dispatch for mouse wheel events.
    /// </summary>
    public bool DispatchMouseWheel(int x, int y, float delta)
    {
        if (_rootScreen == null || !_rootScreen.IsActive)
            return false;

        return DispatchToHierarchyWithHitTest(_rootScreen, x, y, consumer => consumer.OnMouseWheel(x, y, delta));
    }

    /// <summary>
    /// Dispatches to hierarchy depth-first (children first, then parent).
    /// If any consumer handles the input, stops propagation.
    /// </summary>
    private bool DispatchToHierarchy(
        LillyQuest.Engine.Interfaces.Input.IInputConsumer consumer,
        Func<LillyQuest.Engine.Interfaces.Input.IInputConsumer, bool> dispatchAction)
    {
        if (!consumer.IsActive)
            return false;

        // Try children first (depth-first)
        var children = consumer.GetChildren();
        if (children != null)
        {
            foreach (var child in children)
            {
                if (DispatchToHierarchy(child, dispatchAction))
                    return true;  // Consumed by child
            }
        }

        // Then try parent
        return dispatchAction(consumer);
    }

    /// <summary>
    /// Dispatches with hit-testing (children first, then parent).
    /// Only calls dispatchAction if HitTest passes.
    /// </summary>
    private bool DispatchToHierarchyWithHitTest(
        LillyQuest.Engine.Interfaces.Input.IInputConsumer consumer,
        int x,
        int y,
        Func<LillyQuest.Engine.Interfaces.Input.IInputConsumer, bool> dispatchAction)
    {
        if (!consumer.IsActive)
            return false;

        // Try children first (top-to-bottom in hierarchy)
        var children = consumer.GetChildren();
        if (children != null)
        {
            for (int i = children.Count - 1; i >= 0; i--)  // Reverse order = top-most first
            {
                if (DispatchToHierarchyWithHitTest(children[i], x, y, dispatchAction))
                    return true;  // Consumed by child
            }
        }

        // Then try parent if hit-test passes
        if (consumer.HitTest(x, y))
        {
            return dispatchAction(consumer);
        }

        return false;
    }
}
