using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Animations;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Systems;
using LillyQuest.Engine.Types;
using Serilog;

namespace LillyQuest.Engine.Systems;

/// <summary>
/// System for managing and updating animation sequences (tweens).
/// Processes all active tween sequences and removes completed ones.
/// Executes early in the frame before other update systems.
/// </summary>
public class AnimationSystem : ISystem
{
    private List<TweenSequence> _activeSequences = [];
    private readonly ILogger _logger = Log.ForContext<AnimationSystem>();

    public uint Order => 0;
    public string Name => "AnimationSystem";
    public SystemQueryType QueryType => SystemQueryType.Updateable;

    public void Initialize()
    {
        _logger.Information("AnimationSystem initialized");
    }

    /// <summary>
    /// Updates all active animation sequences and removes completed ones.
    /// </summary>
    public void ProcessEntities(GameTime gameTime, IGameEntityManager entityManager)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        foreach (var sequence in _activeSequences.ToList())
        {
            sequence.Update(deltaTime);
            if (sequence.IsComplete)
            {
                _activeSequences.Remove(sequence);
                _logger.Debug("TweenSequence completed, remaining sequences: {Count}", _activeSequences.Count);
            }
        }
    }

    /// <summary>
    /// Creates a new empty tween sequence ready to be configured.
    /// </summary>
    public TweenSequence CreateSequence() => new();

    /// <summary>
    /// Starts playing a tween sequence.
    /// </summary>
    public void Play(TweenSequence sequence)
    {
        _activeSequences.Add(sequence);
        _logger.Debug("Playing TweenSequence with {TweenCount} tween groups", sequence.TweenCount);
    }

    /// <summary>
    /// Stops all currently playing animation sequences.
    /// </summary>
    public void StopAll()
    {
        var count = _activeSequences.Count;
        _activeSequences.Clear();
        _logger.Debug("Stopped all animation sequences ({Count} sequences cleared)", count);
    }

    /// <summary>
    /// Gets the number of currently active sequences.
    /// </summary>
    public int ActiveSequenceCount => _activeSequences.Count;
}
