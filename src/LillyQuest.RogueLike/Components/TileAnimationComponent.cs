using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Json.Entities.Tiles;
using LillyQuest.RogueLike.Types;

namespace LillyQuest.RogueLike.Components;

/// <summary>
/// Component that manages tile-based animations with support for Loop, PingPong, Random, and Once animation types.
/// </summary>
public sealed class TileAnimationComponent
{
    private readonly Random _rng;
    private double _accumulatedTimeMs;
    private int _direction = 1; // 1 = forward, -1 = backward (for PingPong)

    /// <summary>
    /// The animation definition being played.
    /// </summary>
    public TileAnimation Animation { get; }

    /// <summary>
    /// Current frame index in the animation.
    /// </summary>
    public int CurrentFrameIndex { get; private set; }

    /// <summary>
    /// The current animation frame.
    /// </summary>
    public TileAnimationFrame CurrentFrame => Animation.Frames[CurrentFrameIndex];

    /// <summary>
    /// Whether the animation has finished (only true for Once type).
    /// </summary>
    public bool IsFinished { get; private set; }

    public TileAnimationComponent(TileAnimation animation, Random? rng = null)
    {
        ArgumentNullException.ThrowIfNull(animation);
        ArgumentNullException.ThrowIfNull(animation.Frames);

        if (animation.Frames.Count == 0)
        {
            throw new ArgumentException("Animation must have at least one frame", nameof(animation));
        }

        Animation = animation;
        _rng = rng ?? Random.Shared;
    }

    /// <summary>
    /// Resets the animation to the first frame.
    /// </summary>
    public void Reset()
    {
        CurrentFrameIndex = 0;
        _accumulatedTimeMs = 0;
        _direction = 1;
        IsFinished = false;
    }

    /// <summary>
    /// Updates the animation based on elapsed time.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    /// <returns>True if the frame changed, false otherwise.</returns>
    public bool Update(GameTime gameTime)
    {
        if (IsFinished)
        {
            return false;
        }

        _accumulatedTimeMs += gameTime.Elapsed.TotalMilliseconds;

        if (_accumulatedTimeMs < Animation.FrameDurationMs)
        {
            return false;
        }

        _accumulatedTimeMs -= Animation.FrameDurationMs;

        return AdvanceFrame();
    }

    private bool AdvanceFrame()
    {
        var previousIndex = CurrentFrameIndex;

        switch (Animation.Type)
        {
            case TileAnimationType.Loop:
                CurrentFrameIndex = (CurrentFrameIndex + 1) % Animation.Frames.Count;

                break;

            case TileAnimationType.PingPong:
                AdvancePingPong();

                break;

            case TileAnimationType.Random:
                CurrentFrameIndex = _rng.Next(Animation.Frames.Count);

                break;

            case TileAnimationType.Once:
                if (CurrentFrameIndex < Animation.Frames.Count - 1)
                {
                    CurrentFrameIndex++;

                    // Mark as finished when we reach the last frame
                    if (CurrentFrameIndex == Animation.Frames.Count - 1)
                    {
                        IsFinished = true;
                    }
                }

                break;
        }

        return CurrentFrameIndex != previousIndex || Animation.Type == TileAnimationType.Random;
    }

    private void AdvancePingPong()
    {
        var nextIndex = CurrentFrameIndex + _direction;

        if (nextIndex >= Animation.Frames.Count)
        {
            _direction = -1;
            nextIndex = CurrentFrameIndex + _direction;
        }
        else if (nextIndex < 0)
        {
            _direction = 1;
            nextIndex = CurrentFrameIndex + _direction;
        }

        CurrentFrameIndex = nextIndex;
    }
}
