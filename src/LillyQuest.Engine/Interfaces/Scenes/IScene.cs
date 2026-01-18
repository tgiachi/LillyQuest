using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Managers;

namespace LillyQuest.Engine.Interfaces.Scenes;

/// <summary>
/// Interface for a scene in the game world.
/// Scenes contain en otities and manage their lifecycle during scene transitions.
/// </summary>
public interface IScene
{
    /// <summary>
    /// Unique name identifier for this scene (e.g. "main_menu", "level_01").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Returns all entities that belong to this scene.
    /// Used for serialization, debugging, or scene management.
    /// </summary>
    IEnumerable<IGameEntity> GetSceneGameEntities();

    /// <summary>
    /// Called once when the scene is first created/initialized.
    /// Use this for one-time setup like loading assets, creating static entities.
    /// </summary>
    void OnInitialize(ISceneManager sceneManager);

    /// <summary>
    /// Called when the scene becomes active.
    /// Use this for gameplay start, enabling entities, starting music, etc.
    /// </summary>
    void OnLoad();

    /// <summary>
    /// Called when the scene is being unloaded/replaced.
    /// Use this to clean up, save state, stop music, etc.
    /// </summary>
    void OnUnload();

    /// <summary>
    /// Called once on the first activation of the scene to register global game objects.
    /// </summary>
    /// <param name="gameEntityManager">Game object manager used to register globals.</param>
    void RegisterGlobals(IGameEntityManager gameEntityManager);
}
