using System.Numerics;
using ImGuiNET;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Screens.UI;

namespace LillyQuest.Engine.Entities.Debug;

/// <summary>
/// Debug panel that displays the UI control tree for UIRootScreen instances.
/// Highlights the focused control.
/// </summary>
public sealed class DebugUIControlsGameObject : GameEntity, IIMGuiEntity
{
    private readonly IScreenManager _screenManager;

    public DebugUIControlsGameObject(IScreenManager screenManager)
    {
        _screenManager = screenManager;
        IsActive = true;
        Name = "UI Controls";
    }

    public void DrawIMGui()
    {
        var uiScreens = _screenManager.ScreenStack
                                      .OfType<UIRootScreen>()
                                      .ToList();

        ImGui.Text($"UIRoot screens: {uiScreens.Count}");
        ImGui.Separator();

        if (uiScreens.Count == 0)
        {
            ImGui.TextDisabled("No UIRootScreen instances found.");

            return;
        }

        for (var i = uiScreens.Count - 1; i >= 0; i--)
        {
            var screen = uiScreens[i];
            var focused = screen.Root.FocusManager.Focused;
            var nodeOpen = ImGui.TreeNode($"{screen.ConsumerId} (controls: {screen.Root.Children.Count})##ui_{i}");

            if (nodeOpen)
            {
                ImGui.Text("Focused:");
                ImGui.SameLine();

                if (focused != null)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 0.2f, 1f));
                    ImGui.Text(GetControlDisplayName(focused));
                    ImGui.PopStyleColor();
                }
                else
                {
                    ImGui.TextDisabled("None");
                }

                ImGui.Separator();

                var rootChildren = screen.Root.Children.ToList();

                foreach (var control in rootChildren
                                        .OrderByDescending(child => child.ZIndex)
                                        .ThenByDescending(child => rootChildren.IndexOf(child)))
                {
                    DrawControlNode(control, focused, screen.Root.FocusManager);
                }

                ImGui.TreePop();
            }
        }
    }

    private static void DrawControlNode(
        UIScreenControl control,
        UIScreenControl? focused,
        UIFocusManager focusManager
    )
    {
        var isFocused = ReferenceEquals(control, focused);
        var children = GetChildren(control);
        var label = GetControlDisplayName(control);
        var flags = children.Count > 0
                        ? ImGuiTreeNodeFlags.OpenOnArrow
                        : ImGuiTreeNodeFlags.Leaf;

        if (isFocused)
        {
            flags |= ImGuiTreeNodeFlags.Selected;
        }

        if (isFocused)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.2f, 1f, 0.2f, 1f));
        }

        var nodeOpen = ImGui.TreeNodeEx($"{label}##{control.GetHashCode()}", flags);

        if (ImGui.IsItemHovered())
        {
            var world = control.GetWorldPosition();
            ImGui.SetTooltip(
                $"Type: {control.GetType().Name}\nZIndex: {control.ZIndex}\nVisible: {control.IsVisible}\nEnabled: {control.IsEnabled}\nFocusable: {control.IsFocusable}\nPos: {world}\nSize: {control.Size}"
            );
        }

        if (ImGui.IsItemClicked() && control.IsFocusable)
        {
            focusManager.RequestFocus(control);
        }

        if (nodeOpen)
        {
            var childList = children.ToList();

            foreach (var child in childList
                                  .OrderByDescending(child => child.ZIndex)
                                  .ThenByDescending(child => childList.IndexOf(child)))
            {
                DrawControlNode(child, focused, focusManager);
            }

            ImGui.TreePop();
        }

        if (isFocused)
        {
            ImGui.PopStyleColor();
        }
    }

    private static IReadOnlyList<UIScreenControl> GetChildren(UIScreenControl control)
    {
        return control switch
        {
            UIWindow window        => window.Children,
            UIScrollContent scroll => scroll.Children,
            UIButton button        => button.Children,
            _                      => Array.Empty<UIScreenControl>()
        };
    }

    private static string GetControlDisplayName(UIScreenControl control)
    {
        var label = control switch
        {
            UIWindow window when !string.IsNullOrWhiteSpace(window.Title)           => window.Title,
            UIButton button when !string.IsNullOrWhiteSpace(button.Text)            => button.Text,
            UILabel labelControl when !string.IsNullOrWhiteSpace(labelControl.Text) => labelControl.Text,
            _                                                                       => string.Empty
        };

        return !string.IsNullOrWhiteSpace(label) ? $"{control.GetType().Name}: \"{label}\"" : control.GetType().Name;
    }
}
