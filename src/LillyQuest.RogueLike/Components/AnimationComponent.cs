using LillyQuest.Core.Primitives;

namespace LillyQuest.RogueLike.Components;

/// <summary>
/// Component that tracks timing for viewport-based animations.
/// Returns true from Update() when the animation interval is reached,
/// and invokes an optional callback.
/// </summary>
public sealed class AnimationComponent
{
    private double _accumulatedTime;

    /// <summary>
    /// The interval in seconds between animation triggers.
    /// </summary>
    public double IntervalSeconds { get; }

    /// <summary>
    /// Optional callback invoked when the animation interval is reached.
    /// </summary>
    public Action? OnAnimationTrigger { get; set; }

    public AnimationComponent(double intervalSeconds = 1.0, Action? onAnimationTrigger = null)
    {
        IntervalSeconds = intervalSeconds;
        OnAnimationTrigger = onAnimationTrigger;
    }

    /// <summary>
    /// Resets the accumulated time to zero.
    /// </summary>
    public void Reset()
    {
        _accumulatedTime = 0;
    }

    /// <summary>
    /// Updates the accumulated time and returns true if the interval was reached.
    /// If a callback is set, it is invoked when the interval is reached.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    /// <returns>True if the interval was reached and an animation frame should trigger.</returns>
    public bool Update(GameTime gameTime)
    {
        _accumulatedTime += gameTime.Elapsed.TotalSeconds;

        if (_accumulatedTime >= IntervalSeconds)
        {
            _accumulatedTime -= IntervalSeconds;
            OnAnimationTrigger?.Invoke();

            return true;
        }

        return false;
    }
}
