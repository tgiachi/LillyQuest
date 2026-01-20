using ImGuiNET;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Scenes;
using LillyQuest.Game.Screens;
using Serilog;

namespace LillyQuest.Game.Scenes;

/// <summary>
/// Test scene B for validating scene transitions and entity management.
/// </summary>

//TODO: Create BaseScreenScene with common code for scenes that use screens. NB: create List<IScreen> _sceneScreens similar to _sceneEntities to manage screens per scene.
public class TestSceneB : IScene
{
    private readonly ILogger _logger = Log.ForContext<TestSceneB>();
    private readonly List<IGameEntity> _sceneEntities = new();

    private readonly IScreenManager _screenManager;
    private ISceneManager? _sceneManager;

    public TestSceneB(IScreenManager screenManager)
    {
        _screenManager = screenManager;
    }

    public string Name => "test_scene_b";

    public IEnumerable<IGameEntity> GetSceneGameObjects()
        => _sceneEntities;

    public void OnInitialize(ISceneManager sceneManager)
    {
        _sceneManager = sceneManager;
        _logger.Information("TestSceneB initialized");
        _sceneEntities.Add(
            new TestGameEntity(
                "Scene B - Entity 2",
                () =>
                {
                    ImGui.Text("This is Scene A - Entity 1");

                    if (ImGui.Button("Go to Scene B"))
                    {
                        _sceneManager?.SwitchScene("test_scene_a", 2f);
                    }
                }
            )
        );
    }

    public void OnLoad()
    {
        _logger.Information("TestSceneB loaded");

        var testScreen1 = new TestScreen()
        {
            Size = new(200, 200),
            Position = new(100, 100)
        };
        var testScreen2 = new TestScreen
        {
            Size = new(200, 200),
            Position = new(700, 100)
        };

        _screenManager.PushScreen(testScreen1);
        _screenManager.PushScreen(testScreen2);
    }

    public void OnUnload()
    {
        _logger.Information("TestSceneB unloaded");

        _screenManager.PopScreen();
        _screenManager.PopScreen();
    }

    public void RegisterGlobals(IGameEntityManager gameObjectManager)
    {
        _logger.Information("TestSceneB registering globals");

        // Add global entities if needed
    }
}
