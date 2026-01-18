namespace LillyQuest.Engine.Logging;

/// <summary>
/// Dispatches buffered log entries to subscribers.
/// </summary>
public interface ILogEventDispatcher
{
    /// <summary>
    /// Event raised when a batch of log entries is dispatched.
    /// </summary>
    event Action<IReadOnlyList<LogEntry>>? OnLogEntries;

    /// <summary>
    /// Dispatches up to the requested number of log entries.
    /// </summary>
    /// <param name="maxEntries">Maximum entries to dispatch in one call.</param>
    /// <returns>Number of entries dispatched.</returns>
    int Dispatch(int maxEntries = 64);

    /// <summary>
    /// Enqueues a log entry for later dispatch.
    /// </summary>
    /// <param name="entry">The log entry to enqueue.</param>
    void Enqueue(LogEntry entry);
}
