using ImGuiNET;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Managers;

namespace LillyQuest.Engine.Entities.Debug;

/// <summary>
/// Main debug panel that creates and manages all debug objects.
/// Provides a menu bar to toggle visibility of debug panels.
/// All debug objects are created via Dependency Injection.
/// </summary>
public class DebugPanelGameObject : GameEntity, IIMGuiEntity
{
    private readonly Dictionary<string, GameEntity> _debugObjects = new();

    public DebugPanelGameObject(IGameEntityManager entityManager)
    {
        IsActive = true;
        Name = "Debug Panel";

        // Create all debug objects via Dependency Injection
        var debugSystem = entityManager.CreateEntity<DebugSystemGameObject>();
        var debugEntity = entityManager.CreateEntity<DebugEntityGameObject>();
        var debugInput = entityManager.CreateEntity<DebugInputGameObject>();
        var debugLabel = entityManager.CreateEntity<DebugLabelGameObject>();
        var debugScene = entityManager.CreateEntity<DebugSceneExplorerGameObject>();
        var debugScreen = entityManager.CreateEntity<DebugScreenExplorerGameObject>();
        var debugUiControls = entityManager.CreateEntity<DebugUIControlsGameObject>();
        var debugTexture = entityManager.CreateEntity<DebugTextureExplorerGameObject>();
        var debugTile = entityManager.CreateEntity<DebugTileExplorerGameObject>();

        // Set all debug objects inactive by default
        debugSystem.IsActive = false;
        debugEntity.IsActive = false;
        debugInput.IsActive = false;
        debugLabel.IsActive = false;
        debugScene.IsActive = false;
        debugScreen.IsActive = false;
        debugUiControls.IsActive = false;
        debugTexture.IsActive = false;
        debugTile.IsActive = false;

        // Add as children
        AddChild(debugSystem);
        AddChild(debugEntity);
        AddChild(debugInput);
        AddChild(debugLabel);
        AddChild(debugScene);
        AddChild(debugScreen);
        AddChild(debugUiControls);
        AddChild(debugTexture);
        AddChild(debugTile);

        // Register for menu
        _debugObjects["System"] = debugSystem;
        _debugObjects["Entity"] = debugEntity;
        _debugObjects["Input"] = debugInput;
        _debugObjects["Label"] = debugLabel;
        _debugObjects["Scene Explorer"] = debugScene;
        _debugObjects["Screen Explorer"] = debugScreen;
        _debugObjects["UI Controls"] = debugUiControls;
        _debugObjects["Texture Explorer"] = debugTexture;
        _debugObjects["Tile Explorer"] = debugTile;
    }

    public void DrawIMGui()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("Debug"))
            {
                ImGui.Text("Panels:");
                ImGui.Separator();

                // Draw menu items for each debug object
                foreach (var (name, debugObj) in _debugObjects)
                {
                    var isActive = debugObj.IsActive;

                    if (ImGui.MenuItem(name, "", isActive))
                    {
                        debugObj.IsActive = !debugObj.IsActive;
                        ImGui.CloseCurrentPopup();
                    }
                }

                ImGui.Separator();

                // Option to toggle all
                if (ImGui.MenuItem("Enable All"))
                {
                    foreach (var debugObj in _debugObjects.Values)
                    {
                        debugObj.IsActive = true;
                    }
                    ImGui.CloseCurrentPopup();
                }

                if (ImGui.MenuItem("Disable All"))
                {
                    foreach (var debugObj in _debugObjects.Values)
                    {
                        debugObj.IsActive = false;
                    }
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }
    }
}
