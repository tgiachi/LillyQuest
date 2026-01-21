using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Data;
using LillyQuest.Engine.Interfaces.Scenes;
using Silk.NET.Maths;

namespace LillyQuest.Engine.Interfaces.Managers;

/// <summary>
/// Manages scene transitions with fade animations and entity lifecycle.
/// </summary>
public interface ISceneManager
{
    /// <summary>
    /// Gets the currently active scene, or null if no scene is loaded.
    /// </summary>
    IScene? CurrentScene { get; }

    /// <summary>
    /// Gets the current state of the scene transition.
    /// </summary>
    SceneTransitionState TransitionState { get; }

    /// <summary>
    /// Gets all available scenes that have been initialized.
    /// </summary>
    IReadOnlyList<IScene> GetAvailableScenes();

    /// <summary>
    /// Initiates a scene transition with a fade animation.
    /// </summary>
    /// <param name="sceneName">The name of the scene to transition to.</param>
    /// <param name="fadeDuration">Duration of the fade animation in seconds (default 1.0).</param>
    void SwitchScene(string sceneName, float fadeDuration = 1.0f);

    /// <summary>
    /// Updates the scene transition state machine and fade animation.
    /// Called once per frame from the main game loop.
    /// </summary>
    /// <param name="gameTime">Current game time information.</param>
    void Update(GameTime gameTime);

    /// <summary>
    /// Renders the fade overlay during scene transitions.
    /// Should be called after all scene rendering is complete.
    /// </summary>
    /// <param name="screenSize">Size of the screen in pixels.</param>
    void RenderFadeOverlay(Vector2D<int> screenSize);
}
