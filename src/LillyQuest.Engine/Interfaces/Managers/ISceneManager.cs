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
    /// Gets the current active scene. Returns null if the scene stack is empty.
    /// </summary>
    IScene? CurrentScene { get; }

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
    /// <param name="scene"></param>
    void RegisterScene(IScene scene);

    /// <summary>
    /// Sets the current active scene.
    /// </summary>
    void SetCurrentScene(IScene scene);

    /// <summary>
    /// Switches to another scene by name, optionally with a fade duration.
    /// </summary>
    void SwitchScene(string sceneName, float fadeDuration = 1.0f);

    /// <summary>
    /// Gets all scenes in the scene stack (bottom to top).
    /// </summary>
    IReadOnlyList<IScene> SceneStack { get; }

    /// <summary>
    /// Adds a scene to the top of the stack without fade transition.
    /// Only the top scene updates; all scenes render bottom-to-top.
    /// </summary>
    void PushScene(string sceneName);

    /// <summary>
    /// Removes the top scene from the stack. Throws if the stack is empty.
    /// </summary>
    void PopScene();

    /// <summary>
    /// Updates the scene transition state machine.
    /// </summary>
    void Update(GameTime gameTime);
}
