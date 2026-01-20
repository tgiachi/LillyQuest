namespace LillyQuest.Engine.Animations;

/// <summary>
/// Represents a single interpolation animation from start to end value.
/// </summary>
public class Tween
{
    private float _elapsedTime;
    private readonly float _duration;
    private readonly Action<float> _onUpdate;
    private Action? _onComplete;

    /// <summary>
    /// Gets whether this tween has completed its animation.
    /// </summary>
    public bool IsComplete => _elapsedTime >= _duration;

    /// <summary>
    /// Creates a new tween with specified duration and update callback.
    /// </summary>
    /// <param name="duration">Duration of the animation in seconds</param>
    /// <param name="onUpdate">Callback invoked each frame with progress (0-1)</param>
    public Tween(float duration, Action<float> onUpdate)
    {
        _duration = duration;
        _onUpdate = onUpdate;
    }

    /// <summary>
    /// Updates the tween by the given delta time and invokes update callback.
    /// </summary>
    public void Update(float deltaTime)
    {
        _elapsedTime += deltaTime;
        float progress = Math.Min(_elapsedTime / _duration, 1f);
        _onUpdate(progress);

        if (IsComplete)
            _onComplete?.Invoke();
    }

    /// <summary>
    /// Sets the callback to invoke when the tween completes.
    /// </summary>
    public Tween OnComplete(Action callback)
    {
        _onComplete = callback;
        return this;
    }
}
