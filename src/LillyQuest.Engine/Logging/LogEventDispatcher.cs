using System.Collections.Concurrent;

namespace LillyQuest.Engine.Logging;

/// <summary>
/// Buffers log entries and dispatches them on demand.
/// </summary>
public sealed class LogEventDispatcher : ILogEventDispatcher
{
    private readonly ConcurrentQueue<LogEntry> _queue = new();

    public event Action<IReadOnlyList<LogEntry>>? OnLogEntries;

    public int Dispatch(int maxEntries = 64)
    {
        if (maxEntries <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxEntries), "Max entries must be positive.");
        }

        if (!_queue.TryDequeue(out var firstEntry))
        {
            return 0;
        }

        var entries = new List<LogEntry>(maxEntries) { firstEntry };

        while (entries.Count < maxEntries && _queue.TryDequeue(out var entry))
        {
            entries.Add(entry);
        }

        OnLogEntries?.Invoke(entries);

        return entries.Count;
    }

    public void Enqueue(LogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _queue.Enqueue(entry);
    }
}
