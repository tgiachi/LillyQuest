using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Managers;

namespace LillyQuest.Engine.Features;

/// <summary>
/// Renders a full-screen fade overlay during scene transitions.
/// This feature handles the visual transition effect when switching between scenes.
/// </summary>
public class SceneTransitionFeature : IRenderFeature
{
    private readonly ISceneManager _sceneManager;

    /// <summary>
    /// Renders last (on top of everything) so the fade overlay is visible.
    /// </summary>
    public int RenderOrder => int.MaxValue;

    /// <summary>
    /// Whether this feature is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Creates a new SceneTransitionFeature.
    /// </summary>
    public SceneTransitionFeature(ISceneManager sceneManager)
        => _sceneManager = sceneManager;

    /// <summary>
    /// Renders the full-screen fade overlay based on the current transition state.
    /// </summary>
    public void Render(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!IsEnabled)
        {
            return;
        }

        var alpha = _sceneManager.GetFadeAlpha();

        if (alpha <= 0f)
        {
            return;
        }

        // Convert alpha (0-1) to byte (0-255)
        var alphaValue = (byte)(alpha * 255);
        var fadeColor = new LyColor(alphaValue, 0, 0, 0);

        // Get viewport size to draw full screen
        var screenWidth = spriteBatch.Viewport.Size.X;
        var screenHeight = spriteBatch.Viewport.Size.Y;

        // Draw full-screen black rectangle with calculated alpha
        spriteBatch.DrawRectangle(
            new(0, 0),
            new(screenWidth, screenHeight),
            fadeColor,
            float.MaxValue
        );
    }
}
