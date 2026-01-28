using System.Diagnostics;
using LillyQuest.RogueLike.Data.Ticks;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Interfaces.Ticks;
using LillyQuest.RogueLike.Types.Tick;
using Serilog;

namespace LillyQuest.RogueLike.Services;

public class TickSystemService : ITickSystemService
{
    private readonly ILogger _logger = Log.ForContext<TickSystemService>();

    private readonly ActionQueue _actionQueue = new();

    private readonly Queue<ITickAction> _continuingActions = new();

    /// <summary>
    /// Get count of actions waiting to continue
    /// </summary>
    public int ContinuingActionsCount => _continuingActions.Count;

    public int TickCount { get; private set; }

    public event ITickSystemService.TickDelegate? Tick;

    public event ITickSystemService.TickDelegate? TickStarted;

    public event ITickSystemService.TickDelegate? TickEnded;

    /// <summary>
    /// Clear all continuing actions (useful for game reset)
    /// </summary>
    public void ClearContinuingActions()
    {
        var count = _continuingActions.Count;
        _continuingActions.Clear();

        if (count > 0)
        {
            _logger.Information("Cleared {Count} continuing actions", count);
        }
    }

    /// <summary>
    /// Add action to be executed in the next tick
    /// </summary>
    public void EnqueueAction(ITickAction action)
    {
        _actionQueue.Enqueue(action);
    }

    /// <summary>
    /// Add multiple actions to be executed
    /// </summary>
    public void EnqueueActions(IEnumerable<ITickAction> actions)
    {
        _actionQueue.EnqueueRange(actions);
    }

    public void ExecuteTick()
    {
        var startTime = Stopwatch.GetTimestamp();
        TickCount++;
        TickStarted?.Invoke(TickCount);

        RequeueContinuingActions();

        var results = _actionQueue.ExecuteAll();

        ProcessActionResults(results);

        Tick?.Invoke(TickCount);
        TickEnded?.Invoke(TickCount);

        var elapsedMs = Stopwatch.GetElapsedTime(startTime);

        _logger.Debug(
            "Tick completed #{TickCount} with {ActionCount} actions elapsed in {ElapsedMs} ms",
            TickCount,
            results.Count,
            elapsedMs.TotalMilliseconds
        );
    }

    private void ProcessActionResults(List<ActionExecutionSummary> results)
    {
        foreach (var result in results)
        {
            if (result.Result == ActionResult.Continuing)
            {
                _continuingActions.Enqueue(result.Action);

                _logger.Verbose("Action {ActionId} will continue in next tick", result.ActionId);
            }

            if (result.Result == ActionResult.Failed)
            {
                _logger.Warning(
                    "Action {ActionId} failed with error",
                    result.ActionId
                );
            }
        }
    }

    private void RequeueContinuingActions()
    {
        while (_continuingActions.Count > 0)
        {
            var continuingAction = _continuingActions.Dequeue();
            _actionQueue.Enqueue(continuingAction);

            _logger.Verbose(
                "Requeued continuing action: {ActionType} (ID: {ActionId})",
                continuingAction.GetType().Name,
                continuingAction.Id
            );
        }
    }
}
