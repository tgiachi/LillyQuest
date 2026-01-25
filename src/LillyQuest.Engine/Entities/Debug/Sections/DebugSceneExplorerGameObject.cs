using System.Numerics;
using ImGuiNET;
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Managers;

namespace LillyQuest.Engine.Entities.Debug;

/// <summary>
/// Debug object that displays the current scene with all its children and available scenes.
/// </summary>
public class DebugSceneExplorerGameObject : GameEntity, IIMGuiEntity
{
    private readonly ISceneManager _sceneManager;

    public DebugSceneExplorerGameObject(ISceneManager sceneManager)
    {
        _sceneManager = sceneManager;
        IsActive = true;
        Name = "Scene Explorer";
    }

    public void DrawIMGui()
    {
        ImGui.Text("Current Scene:");
        ImGui.Separator();

        var currentScene = _sceneManager.CurrentScene;

        if (currentScene == null)
        {
            ImGui.TextDisabled("No scene loaded");

            return;
        }

        ImGui.Text($"Scene: {currentScene.Name}");
        ImGui.Spacing();

        // Display scene entities
        ImGui.Text("Scene Entities:");
        var sceneEntities = currentScene.GetSceneGameObjects().ToList();
        ImGui.Text($"Total: {sceneEntities.Count}");

        if (ImGui.BeginChild("SceneEntitiesChild", new(0, 150)))
        {
            DrawEntityHierarchy(sceneEntities);
            ImGui.EndChild();
        }

        ImGui.Spacing();
        ImGui.Separator();

        // Display available scenes
        ImGui.Text("Available Scenes:");
        var registeredSceneNames = _sceneManager.GetRegisteredSceneNames().ToList();
        ImGui.Text($"Total: {registeredSceneNames.Count}");

        if (ImGui.BeginChild("ScenesListChild", new(0, 150)))
        {
            foreach (var sceneName in registeredSceneNames)
            {
                var isCurrent = currentScene != null && sceneName == currentScene.Name;

                if (isCurrent)
                {
                    ImGui.TextColored(new(0.2f, 1.0f, 0.2f, 1.0f), $"[*] {sceneName}");
                }
                else
                {
                    if (ImGui.Selectable($"    {sceneName}"))
                    {
                        _sceneManager.SwitchScene(sceneName, 0.5f);
                    }
                }
            }
            ImGui.EndChild();
        }
    }

    private void DrawEntityHierarchy(List<IGameEntity> entities)
    {
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
