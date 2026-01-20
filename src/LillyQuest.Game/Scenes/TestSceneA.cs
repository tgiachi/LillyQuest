using ImGuiNET;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Managers.Scenes.Base;

namespace LillyQuest.Game.Scenes;

/// <summary>
/// Test scene A for validating scene transitions and entity management.
/// </summary>
public class TestSceneA : BaseScene
{
    public TestSceneA() : base("test_scene_a") { }

    public override void OnInitialize(ISceneManager sceneManager)
    {
        AddEntity(
            new TestGameEntity(
                "Test Scene A - Entity 1",
                () =>
                {
                    if (ImGui.Button("Go to Scene B"))
                    {
                        sceneManager.SwitchScene("test_scene_b", 2f);
                    }
                }
            )
        );

        AddEntity(new Test2GameEntity());

        base.OnInitialize(sceneManager);
    }
}
