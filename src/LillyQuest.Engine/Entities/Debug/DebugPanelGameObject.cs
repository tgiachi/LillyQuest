using ImGuiNET;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Services;
using LillyQuest.Engine.Systems;

namespace LillyQuest.Engine.Entities.Debug;

/// <summary>
/// Main debug panel that creates and manages all debug objects.
/// Provides a menu bar to toggle visibility of debug panels.
/// </summary>
public class DebugPanelGameObject : GameEntity, IIMGuiEntity
{
    private readonly Dictionary<string, GameEntity> _debugObjects = new();

    public DebugPanelGameObject(
        LillyQuestBootstrap bootstrap,
        ISystemManager systemManager,
        IGameEntityManager entityManager,
        ISceneManager sceneManager,
        IScreenManager screenManager,
        ITextureManager textureManager,
        ITilesetManager tilesetManager,
        InputSystem inputSystem,
        EngineRenderContext renderContext)
    {
        IsActive = true;
        Name = "Debug Panel";

        // Create all debug objects
        var debugSystem = new DebugSystemGameObject(bootstrap, systemManager);
        var debugEntity = new DebugEntityGameObject(entityManager);
        var debugInput = new DebugInputGameObject(inputSystem);
        var debugLabel = new DebugLabelGameObject(renderContext);
        var debugScene = new DebugSceneExplorerGameObject(sceneManager);
        var debugScreen = new DebugScreenExplorerGameObject(screenManager);
        var debugTexture = new DebugTextureExplorerGameObject(textureManager);
        var debugTile = new DebugTileExplorerGameObject(tilesetManager);

        // Set all debug objects inactive by default
        debugSystem.IsActive = false;
        debugEntity.IsActive = false;
        debugInput.IsActive = false;
        debugLabel.IsActive = false;
        debugScene.IsActive = false;
        debugScreen.IsActive = false;
        debugTexture.IsActive = false;
        debugTile.IsActive = false;

        // Add as children
        AddChild(debugSystem);
        AddChild(debugEntity);
        AddChild(debugInput);
        AddChild(debugLabel);
        AddChild(debugScene);
        AddChild(debugScreen);
        AddChild(debugTexture);
        AddChild(debugTile);

        // Register for menu
        _debugObjects["System"] = debugSystem;
        _debugObjects["Entity"] = debugEntity;
        _debugObjects["Input"] = debugInput;
        _debugObjects["Label"] = debugLabel;
        _debugObjects["Scene Explorer"] = debugScene;
        _debugObjects["Screen Explorer"] = debugScreen;
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
                    bool isActive = debugObj.IsActive;
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
