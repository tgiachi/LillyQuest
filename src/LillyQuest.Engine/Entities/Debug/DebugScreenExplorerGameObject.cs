using System.Numerics;
using ImGuiNET;
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Managers;

namespace LillyQuest.Engine.Entities.Debug;

/// <summary>
/// Debug object that displays the current screen stack, focused screen, and their child entities.
/// </summary>
public class DebugScreenExplorerGameObject : GameEntity, IIMGuiEntity
{
    private readonly IScreenManager _screenManager;

    public DebugScreenExplorerGameObject(IScreenManager screenManager)
    {
        _screenManager = screenManager;
        IsActive = true;
        Name = "Screen Explorer";
    }

    public void DrawIMGui()
    {
        ImGui.Text("Screen Stack:");
        ImGui.Separator();

        var screenStack = _screenManager.ScreenStack;
        ImGui.Text($"Total Screens: {screenStack.Count}");
        ImGui.Spacing();

        if (screenStack.Count == 0)
        {
            ImGui.TextDisabled("No screens in stack");

            return;
        }

        // Display focused screen
        var focusedScreen = _screenManager.FocusedScreen;

        if (focusedScreen != null)
        {
            ImGui.Text("Focused Screen:");
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.2f, 1.0f, 0.2f, 1.0f));
            ImGui.Text($"[*] {focusedScreen.ConsumerId}");
            ImGui.PopStyleColor();
            ImGui.Spacing();

            // Display focused screen entities
            ImGui.Text("Focused Screen Entities:");
            var focusedEntities = focusedScreen.GetScreenGameObjects().ToList();
            ImGui.Text($"Total: {focusedEntities.Count}");

            if (ImGui.BeginChild("FocusedScreenEntitiesChild", new(0, 150)))
            {
                DrawEntitiesList(focusedEntities);
                ImGui.EndChild();
            }

            ImGui.Spacing();
            ImGui.Separator();
        }

        // Display all screens in stack (bottom to top)
        ImGui.Text("Screen Stack (bottom to top):");

        if (ImGui.BeginChild("ScreenStackChild", new(0, 200)))
        {
            var screenList = screenStack.ToList();

            for (var i = screenList.Count - 1; i >= 0; i--)
            {
                var screen = screenList[i];
                var isFocused = screen == focusedScreen;
                var entities = screen.GetScreenGameObjects().ToList();

                var prefix = isFocused ? "> " : "  ";

                if (isFocused)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 0.2f, 1.0f));
                }

                var nodeOpen = ImGui.TreeNode($"{prefix}{screen.ConsumerId} (entities: {entities.Count})##screen_{i}");

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(
                        $"Active: {screen.IsActive}\nModal: {screen.IsModal}\nPosition: {screen.Position}\nSize: {screen.Size}"
                    );
                }

                if (nodeOpen)
                {
                    // Draw entities of this screen
                    DrawEntitiesList(entities);
                    ImGui.TreePop();
                }

                if (isFocused)
                {
                    ImGui.PopStyleColor();
                }
            }
            ImGui.EndChild();
        }
    }

    private void DrawEntitiesList(List<IGameEntity> entities)
    {
        if (entities.Count == 0)
        {
            ImGui.TextDisabled("No entities");

            return;
        }

        foreach (var entity in entities)
        {
            DrawEntityNode(entity);
        }
    }

    private void DrawEntityNode(IGameEntity entity)
    {
        var hasChildren = entity.Children.Count > 0;
        var isActive = entity.IsActive;

        // Color inactive entities gray
        if (!isActive)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
        }

        var label = hasChildren ? $"[{entity.Children.Count}]" : "o";
        var nodeOpen = ImGui.TreeNode($"{entity.Name} ({entity.Id}) {label}");

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                $"ID: {entity.Id}\nOrder: {entity.Order}\nActive: {isActive}\nChildren: {entity.Children.Count}"
            );
        }

        if (nodeOpen)
        {
            // Draw children recursively
            foreach (var child in entity.Children)
            {
                DrawEntityNode(child);
            }
            ImGui.TreePop();
        }

        if (!isActive)
        {
            ImGui.PopStyleColor();
        }
    }
}
