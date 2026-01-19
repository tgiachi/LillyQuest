namespace LillyQuest.Engine.Interfaces.GameObjects.Features;

/// <summary>
/// Base interface for all game object features.
/// Features are composable behaviors and data that can be attached to game objects.
/// </summary>
public interface IGameObjectFeature
{
    /// <summary>
    /// Gets or sets whether this feature is currently enabled.
    /// </summary>
    bool IsEnabled { get; set; }
}
