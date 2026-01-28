// ActionQueue implementation for ITickAction

using LillyQuest.RogueLike.Interfaces.Ticks;
using LillyQuest.RogueLike.Types.Tick;
using Serilog;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace LillyQuest.RogueLike.Data.Ticks;

/// <summary>
/// Manages the queue of actions for a single tick with execution logic
/// </summary>
public class ActionQueue
{
    private readonly List<ITickAction> _actions = new();
    private readonly ILogger _logger = Log.ForContext<ActionQueue>();

    /// <summary>
    /// Get count of queued actions
    /// </summary>
    public int Count => _actions.Count;

    /// <summary>
    /// Check if queue is empty
    /// </summary>
    public bool IsEmpty => _actions.Count == 0;

    /// <summary>
    /// Clear all actions from the queue
    /// </summary>
    public void Clear()
    {
        var count = _actions.Count;
        _actions.Clear();

        if (count > 0)
        {
            _logger.Verbose("Cleared {ActionCount} actions from queue", count);
        }
    }

    /// <summary>
    /// Check if queue contains specific action
    /// </summary>
    /// <param name="action">Action to check for</param>
    /// <returns>True if action is in queue</returns>
    public bool Contains(ITickAction action)
        => _actions.Contains(action);

    /// <summary>
    /// Check if queue contains action with specific ID
    /// </summary>
    /// <param name="actionId">Action ID to check for</param>
    /// <returns>True if action with ID is in queue</returns>
    public bool ContainsId(Guid actionId)
    {
        return _actions.Any(a => a.Id == actionId);
    }

    /// <summary>
    /// Add action to the queue
    /// </summary>
    /// <param name="action">Action to enqueue</param>
    public void Enqueue(ITickAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        _actions.Add(action);
        _logger.Verbose(
            "Enqueued action: {ActionType} (ID: {ActionId}, Priority: {Priority}, Speed: {Speed})",
            action.GetType().Name,
            action.Id,
            action.Priority,
            action.Speed
        );
    }

    /// <summary>
    /// Add multiple actions to the queue
    /// </summary>
    /// <param name="actions">Actions to enqueue</param>
    public void EnqueueRange(IEnumerable<ITickAction> actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        foreach (var action in actions)
        {
            Enqueue(action);
        }
    }

    /// <summary>
    /// Execute all actions in priority order and return results
    /// </summary>
    /// <returns>List of execution results with their corresponding actions</returns>
    public List<ActionExecutionSummary> ExecuteAll()
    {
        var results = new List<ActionExecutionSummary>();
        var sortedActions = GetSortedActions();

        _logger.Verbose("Executing {ActionCount} actions", sortedActions.Count);

        foreach (var action in sortedActions)
        {
            var result = ExecuteSingleAction(action);
            results.Add(result);

            // If action failed critically, might want to stop processing
            if (result.Result == ActionResult.Invalid)
            {
                _logger.Warning("Action {ActionId} returned Invalid result, continuing with next actions", action.Id);
            }
        }

        // Clear queue after execution
        Clear();

        _logger.Verbose("Completed executing {ActionCount} actions", results.Count);

        return results;
    }

    /// <summary>
    /// Execute actions one by one, allowing for early termination
    /// </summary>
    /// <returns>Enumerable of execution results as they happen</returns>
    public IEnumerable<ActionExecutionSummary> ExecuteSequentially()
    {
        var sortedActions = GetSortedActions();

        foreach (var action in sortedActions)
        {
            var result = ExecuteSingleAction(action);

            yield return result;
        }

        Clear();
    }

    /// <summary>
    /// Get actions by priority level
    /// </summary>
    /// <param name="priority">Priority level to filter by</param>
    /// <returns>Actions with specified priority</returns>
    public List<ITickAction> GetActionsByPriority(ActionPriority priority)
    {
        return _actions.Where(a => a.Priority == priority).ToList();
    }

    /// <summary>
    /// Get all actions without sorting (for inspection)
    /// </summary>
    /// <returns>Read-only collection of all actions</returns>
    public IReadOnlyList<ITickAction> GetAllActions()
        => _actions.AsReadOnly();

    /// <summary>
    /// Get all actions sorted by execution order without executing them
    /// </summary>
    /// <returns>Actions sorted by Priority -> Speed (desc) -> Id</returns>
    public List<ITickAction> GetSortedActions()
    {
        return _actions
               .OrderBy(a => (int)a.Priority)  // Lower priority value = higher priority
               .ThenByDescending(a => a.Speed) // Higher speed = goes first
               .ThenBy(a => a.Id)              // Deterministic ordering for ties
               .ToList();
    }

    /// <summary>
    /// Preview next action to be executed without executing it
    /// </summary>
    /// <returns>Next action or null if queue is empty</returns>
    public ITickAction? PeekNext()
        => _actions.Count > 0 ? GetSortedActions().First() : null;

    /// <summary>
    /// Remove specific action from queue
    /// </summary>
    /// <param name="action">Action to remove</param>
    /// <returns>True if action was found and removed</returns>
    public bool Remove(ITickAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var removed = _actions.Remove(action);

        if (removed)
        {
            _logger.Verbose(
                "Removed action: {ActionType} (ID: {ActionId})",
                action.GetType().Name,
                action.Id
            );
        }

        return removed;
    }

    /// <summary>
    /// Remove action by ID
    /// </summary>
    /// <param name="actionId">ID of action to remove</param>
    /// <returns>True if action was found and removed</returns>
    public bool RemoveById(Guid actionId)
    {
        var action = _actions.FirstOrDefault(a => a.Id == actionId);

        return action != null && Remove(action);
    }

    /// <summary>
    /// Remove all actions matching a predicate
    /// </summary>
    /// <param name="predicate">Condition for removal</param>
    /// <returns>Number of actions removed</returns>
    public int RemoveWhere(Func<ITickAction, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var toRemove = _actions.Where(predicate).ToList();
        var removedCount = 0;

        foreach (var action in toRemove)
        {
            if (_actions.Remove(action))
            {
                removedCount++;
                _logger.Verbose(
                    "Removed action by predicate: {ActionType} (ID: {ActionId})",
                    action.GetType().Name,
                    action.Id
                );
            }
        }

        return removedCount;
    }

    /// <summary>
    /// Execute a single action with proper error handling and logging
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <returns>Execution summary</returns>
    private ActionExecutionSummary ExecuteSingleAction(ITickAction action)
    {
        var startTimestamp = Stopwatch.GetTimestamp();

        try
        {
            _logger.Verbose(
                "Executing action: {ActionType} (ID: {ActionId})",
                action.GetType().Name,
                action.Id
            );

            // Check if action can be executed
            if (!action.CanBeExecuted())
            {
                _logger.Verbose("Action {ActionId} cannot be executed, skipping", action.Id);

                return new(
                    action,
                    ActionResult.Blocked,
                    startTimestamp,
                    Stopwatch.GetTimestamp(),
                    "Action cannot be executed"
                );
            }

            // Execute the action
            var result = action.Execute();
            var endTimestamp = Stopwatch.GetTimestamp();
            var duration = Stopwatch.GetElapsedTime(startTimestamp, endTimestamp);

            _logger.Verbose(
                "Action {ActionId} executed with result: {Result} (Duration: {Duration}ms)",
                action.Id,
                result,
                duration.TotalMilliseconds
            );

            return new(action, result, startTimestamp, endTimestamp);
        }
        catch (Exception ex)
        {
            var endTimestamp = Stopwatch.GetTimestamp();
            _logger.Error(ex, "Error executing action {ActionId}: {Error}", action.Id, ex.Message);

            return new(
                action,
                ActionResult.Failed,
                startTimestamp,
                endTimestamp,
                $"Exception: {ex.Message}"
            );
        }
    }
}
