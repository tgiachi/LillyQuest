using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Scenes;

namespace LillyQuest.Engine.Managers.Scenes.Base;

public abstract class BaseScene : IScene
{
    protected ISceneManager SceneManager { get; private set; }
    public string Name { get; }

    private readonly List<IGameEntity> _sceneGameObjects = [];

    private readonly List<IGameEntity> _globalGameObjects = [];

    protected BaseScene(string name)
    {
        Name = name;
    }

    protected void AddEntity(IGameEntity entity)
    {
        entity.Name = $"Scene_{Name}_{entity.Name}";
        _sceneGameObjects.Add(entity);
    }

    public IEnumerable<IGameEntity> GetSceneGameObjects()
        => _sceneGameObjects;

    public virtual void OnInitialize(ISceneManager sceneManager)
    {
        SceneManager = sceneManager;
    }

    public virtual void OnLoad() { }
    public virtual void OnUnload() { }

    public void RegisterGlobals(IGameEntityManager gameObjectManager)
    {
        foreach (var global in _globalGameObjects)
        {
            gameObjectManager.AddEntity(global);
        }
    }
}
