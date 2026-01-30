namespace LillyQuest.Engine.Interfaces.Services;

/// <summary>
/// Dispatches work onto the main thread.
/// </summary>
public interface IMainThreadDispatcher
{
    /// <summary>
    /// Gets the managed thread id of the main thread.
    /// </summary>
    int MainThreadId { get; }

    /// <summary>
    /// Gets whether the caller is on the main thread.
    /// </summary>
    bool IsMainThread { get; }

    /// <summary>
    /// Maximum number of actions to execute per frame.
    /// </summary>
    int MaxActionsPerFrame { get; set; }

    /// <summary>
    /// Executes pending work items on the main thread.
    /// </summary>
    /// <param name="maxActions">Max actions to execute, or null to use MaxActionsPerFrame.</param>
    /// <returns>Number of actions executed.</returns>
    int ExecutePending(int? maxActions = null);

    /// <summary>
    /// Executes an action on the main thread and blocks until completion.
    /// </summary>
    void Invoke(Action action);

    /// <summary>
    /// Executes a function on the main thread and blocks until completion.
    /// </summary>
    T Invoke<T>(Func<T> func);

    /// <summary>
    /// Enqueues a fire-and-forget action to run on the main thread.
    /// </summary>
    void Post(Action action);
}
