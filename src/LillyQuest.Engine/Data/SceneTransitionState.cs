namespace LillyQuest.Engine.Data;

/// <summary>
/// Represents the current state of a scene transition.
/// </summary>
public enum SceneTransitionState
{
    /// <summary>No transition in progress.</summary>
    Idle,

    /// <summary>Fading to black before scene switch.</summary>
    FadingOut,

    /// <summary>Actually switching scenes (loading/unloading).</summary>
    Loading,

    /// <summary>Fading from black after scene switch.</summary>
    FadingIn
}
