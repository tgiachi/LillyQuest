using ImGuiNET;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Scenes;
using Serilog;

namespace LillyQuest.Game.Scenes;

/// <summary>
/// Test scene A for validating scene transitions and entity management.
/// </summary>
public class TestSceneA : IScene
{
    private readonly ILogger _logger = Log.ForContext<TestSceneA>();
    private readonly List<IGameEntity> _sceneEntities = new();
    private ISceneManager? _sceneManager;

    public string Name => "test_scene_a";

    public IEnumerable<IGameEntity> GetSceneGameObjects()
        => _sceneEntities;

    public void OnInitialize(ISceneManager sceneManager)
    {
        _sceneManager = sceneManager;
        _logger.Information("TestSceneA initialized");
        _sceneEntities.Add(
            new TestGameEntity(
                "Scene A - Entity 1",
                () =>
                {
                    ImGui.Text("This is Scene A - Entity 1");

                    if (ImGui.Button("Go to Scene B"))
                    {
                        _sceneManager?.SwitchScene("test_scene_b", 2f);
                    }
                }
            )
        );
    }

    public void OnLoad()
    {
        _logger.Information("TestSceneA loaded");
    }

    public void OnUnload()
    {
        _logger.Information("TestSceneA unloaded");
    }

    public void RegisterGlobals(IGameEntityManager gameObjectManager)
    {
        _logger.Information("TestSceneA registering globals");

        // Add global entities if needed
    }
}
