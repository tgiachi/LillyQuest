using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Scenes;
using LillyQuest.Engine.Interfaces.Screens;

namespace LillyQuest.Engine.Managers.Screens.Base;

public abstract class BaseScreenScene : IScene
{
    public string Name { get; }

    private readonly List<IGameEntity> _sceneEntities = [];
    private readonly List<IScreen> _sceneScreens = [];
    private readonly List<IGameEntity> _globalEntities = [];

    protected ISceneManager SceneManager { get; private set; }
    protected IScreenManager ScreenManager { get; }

    protected BaseScreenScene(string name, IScreenManager screenManager)
    {
        Name = name;
        ScreenManager = screenManager;
    }

    public IEnumerable<IGameEntity> GetSceneGameObjects()
        => _sceneEntities;

    public void OnInitialize(ISceneManager sceneManager)
    {
        SceneManager = sceneManager;

        SceneInitialized();

        foreach (var entity in _sceneScreens)
        {
            ScreenManager.PushScreen(entity);
        }
    }

    public void OnLoad()
    {
        throw new NotImplementedException();
    }

    public void OnUnload()
    {
        foreach (var stackScene in ScreenManager.ScreenStack)
        {
            ScreenManager.PopScreen(stackScene);
        }
    }

    public void RegisterGlobals(IGameEntityManager gameObjectManager)
    {
        foreach (var global in _globalEntities)
        {
            gameObjectManager.AddEntity(global);
        }
    }

    protected void AddEntity(IGameEntity entity)
    {
        _sceneEntities.Add(entity);
    }

    protected void AddScreen(IScreen screen)
    {
        _sceneScreens.Add(screen);
    }

    protected void RemoveEntity(IGameEntity entity)
    {
        _sceneEntities.Remove(entity);
    }

    protected void RemoveScreen(IScreen screen)
    {
        _sceneScreens.Remove(screen);
    }

    protected void AddGlobalEntity(IGameEntity entity)
    {
        _globalEntities.Add(entity);
    }

    protected void RemoveGlobalEntity(IGameEntity entity)
    {
        _globalEntities.Remove(entity);
    }

    protected virtual void SceneInitialized() { }
}
