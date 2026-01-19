using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Scenes;

namespace LillyQuest.Engine.Scenes.Base;

/// <summary>
/// Base abstract class for scenes providing default implementations of IScene.
/// Handles storing scene entities and lifecycle method delegation.
/// </summary>
public abstract class BaseScene : IScene
{
    /// <summary>
    /// Unique name identifier for this scene (e.g. "main_menu", "level_01").
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Collection of all entities that belong to this scene.
    /// </summary>
    protected List<IGameEntity> SceneGameEntities { get; } = new();

    /// <summary>
    /// Reference to the scene manager, set during OnInitialize.
    /// </summary>
    protected ISceneManager? SceneManager { get; private set; }

    /// <summary>
    /// Returns all entities that belong to this scene.
    /// </summary>
    public IEnumerable<IGameEntity> GetSceneGameEntities()
        => SceneGameEntities;

    /// <summary>
    /// Called once when the scene is first created/initialized.
    /// Override this for one-time setup like loading assets, creating static entities.
    /// </summary>
    public virtual void OnInitialize(ISceneManager sceneManager)
    {
        SceneManager = sceneManager;
    }

    /// <summary>
    /// Called when the scene becomes active.
    /// Override this for gameplay start, enabling entities, starting music, etc.
    /// </summary>
    public virtual void OnLoad() { }

    /// <summary>
    /// Called when the scene is being unloaded/replaced.
    /// Override this to clean up, save state, stop music, etc.
    /// </summary>
    public virtual void OnUnload() { }

    /// <summary>
    /// Called once on the first activation of the scene to register global game objects.
    /// Override this to register entities that should persist across scene changes.
    /// </summary>
    public virtual void RegisterGlobals(IGameEntityManager gameEntityManager) { }

    /// <summary>
    /// Adds an entity to this scene's collection.
    /// </summary>
    protected void AddEntity(IGameEntity entity)
    {
        SceneGameEntities.Add(entity);
    }

    /// <summary>
    /// Removes an entity from this scene's collection.
    /// </summary>
    protected void RemoveEntity(IGameEntity entity)
    {
        SceneGameEntities.Remove(entity);
    }
}
