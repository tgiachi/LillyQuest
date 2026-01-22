namespace LillyQuest.Engine.Animations;

/// <summary>
/// Represents a sequence of tweens that can be played sequentially or in parallel.
/// </summary>
public class TweenSequence
{
    private readonly Queue<(List<Tween> tweens, bool isParallel)> _queue = [];
    private List<Tween>? _currentGroup;
    private bool _hasStarted;
    private Action? _onComplete;

    /// <summary>
    /// Gets the number of tween groups in the sequence.
    /// </summary>
    public int TweenCount => _queue.Count;

    /// <summary>
    /// Gets whether this sequence has completed all tweens.
    /// </summary>
    public bool IsComplete => _hasStarted && _currentGroup == null && _queue.Count == 0;

    /// <summary>
    /// Appends a single tween to be played after all previous tweens complete.
    /// </summary>
    public TweenSequence Append(Tween tween)
    {
        _queue.Enqueue((new() { tween }, false));

        return this;
    }

    /// <summary>
    /// Sets the callback to invoke when the entire sequence completes.
    /// </summary>
    public TweenSequence OnComplete(Action callback)
    {
        _onComplete = callback;

        return this;
    }

    /// <summary>
    /// Appends multiple tweens to be played in parallel after all previous tweens complete.
    /// </summary>
    public TweenSequence Parallel(params Tween[] tweens)
    {
        _queue.Enqueue((tweens.ToList(), true));

        return this;
    }

    /// <summary>
    /// Updates all active tweens in the current group by the given delta time.
    /// </summary>
    public void Update(float deltaTime)
    {
        if (!_hasStarted)
        {
            _hasStarted = true;
        }

        if (_currentGroup == null && _queue.Count > 0)
        {
            var (tweens, _) = _queue.Dequeue();
            _currentGroup = tweens;
        }

        if (_currentGroup != null)
        {
            foreach (var tween in _currentGroup)
            {
                tween.Update(deltaTime);
            }

            if (_currentGroup.All(t => t.IsComplete))
            {
                _currentGroup = null;

                if (_queue.Count == 0)
                {
                    _onComplete?.Invoke();
                }
            }
        }
    }
}
