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

        if (ImGui.BeginChild("SceneEntitiesChild", new System.Numerics.Vector2(0, 150)))
        {
            DrawEntityHierarchy(sceneEntities);
            ImGui.EndChild();
        }

        ImGui.Spacing();
        ImGui.Separator();

        // Display available scenes
        ImGui.Text("Available Scenes:");
        var availableScenes = _sceneManager.GetAvailableScenes().ToList();
        ImGui.Text($"Total: {availableScenes.Count}");

        if (ImGui.BeginChild("ScenesListChild", new System.Numerics.Vector2(0, 150)))
        {
            foreach (var scene in availableScenes)
            {
                var isCurrent = scene == currentScene;
                if (isCurrent)
                {
                    ImGui.TextColored(new System.Numerics.Vector4(0.2f, 1.0f, 0.2f, 1.0f), $"[*] {scene.Name}");
                }
                else
                {
                    ImGui.Text($"    {scene.Name}");
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
            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.5f, 0.5f, 0.5f, 1.0f));
        }

        string label = hasChildren ? $"[{entity.Children.Count}]" : "o";
        bool nodeOpen = ImGui.TreeNode($"{entity.Name} ({entity.Id}) {label}");

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip($"ID: {entity.Id}\nOrder: {entity.Order}\nActive: {isActive}\nChildren: {entity.Children.Count}");
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
