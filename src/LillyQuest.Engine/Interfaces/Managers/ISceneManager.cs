using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Scenes;

namespace LillyQuest.Engine.Interfaces.Managers;

/// <summary>
/// Manages scene loading, switching, and change notifications.
/// </summary>
public interface ISceneManager
{
    /// <summary>
    /// Handler invoked after a scene switch completes.
    /// </summary>
    delegate void SceneChangedHandler(IScene newScene);

    /// <summary>
    /// Handler invoked when a scene starts loading.
    /// </summary>
    delegate void SceneLoadingHandler(IScene scene);

    /// <summary>
    /// Raised after a scene switch completes.
    /// </summary>
    event SceneChangedHandler? OnSceneChanged;

    /// <summary>
    /// Raised when a scene starts loading.
    /// </summary>
    event SceneLoadingHandler? OnSceneLoading;

    /// <summary>
    /// Gets the current active scene (top of stack), or null if stack is empty.
    /// </summary>
    IScene? CurrentScene { get; }

    /// <summary>
    /// Gets all scenes currently in the stack (bottom to top).
    /// </summary>
    IReadOnlyList<IScene> SceneStack { get; }

    /// <summary>
    /// Returns the current fade alpha (0-1) for transitions.
    /// </summary>
    float GetFadeAlpha();

    /// <summary>
    /// Loads a scene by name without activating it.
    /// </summary>
    IScene LoadScene(string sceneName);

    /// <summary>
    /// Registers a scene with the scene manager.
    /// </summary>
    void RegisterScene(IScene scene);

    /// <summary>
    /// Sets the current active scene without any transition or stack manipulation.
    /// Used internally during scene initialization.
    /// </summary>
    void SetCurrentScene(IScene scene);

    /// <summary>
    /// Switches to another scene by name with fade transition.
    /// Clears the entire stack and pushes the new scene.
    /// Fires OnSceneChanged with the new scene once transition completes.
    /// </summary>
    void SwitchScene(string sceneName, float fadeDuration = 1.0f);

    /// <summary>
    /// Pushes a scene onto the stack without fade transition.
    /// Calls RegisterGlobals() once per scene (persists across entire session).
    /// Calls OnLoad() on the scene and fires OnSceneChanged with the new top scene.
    /// Adds all scene entities to the EntityManager.
    /// The previous top scene becomes paused in the background.
    /// </summary>
    void PushScene(string sceneName);

    /// <summary>
    /// Pops the top scene from the stack.
    /// Calls OnUnload() on the removed scene and removes its entities from the EntityManager.
    /// Fires OnSceneChanged with the new top scene (if stack becomes non-empty).
    /// CurrentScene is updated immediately to reflect the new top of stack (or null if stack becomes empty).
    /// Throws InvalidOperationException if the stack is empty.
    /// </summary>
    void PopScene();

    /// <summary>
    /// Updates the scene transition state machine.
    /// </summary>
    void Update(GameTime gameTime);
}
