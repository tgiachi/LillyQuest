using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Screens;
using LillyQuest.Engine.Managers.Scenes.Base;

namespace LillyQuest.Engine.Managers.Screens.Base;

public abstract class BaseScreenScene : BaseScene
{
    private readonly List<IScreen> _sceneScreens = [];

    protected IScreenManager ScreenManager { get; }

    protected BaseScreenScene(string name, IScreenManager screenManager)
        : base(name)
        => ScreenManager = screenManager;

    public override void OnInitialize(ISceneManager sceneManager)
    {
        base.OnInitialize(sceneManager);

        SceneInitialized();

        foreach (var screen in _sceneScreens)
        {
            ScreenManager.PushScreen(screen);
        }
    }

    public override void OnUnload()
    {
        foreach (var screen in ScreenManager.ScreenStack.ToList())
        {
            ScreenManager.PopScreen(screen);
        }

        base.OnUnload();
    }

    protected void AddScreen(IScreen screen)
    {
        _sceneScreens.Add(screen);
    }

    protected void RemoveScreen(IScreen screen)
    {
        _sceneScreens.Remove(screen);
    }

    protected virtual void SceneInitialized() { }
}
