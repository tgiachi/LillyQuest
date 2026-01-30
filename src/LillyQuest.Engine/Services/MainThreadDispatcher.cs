using System.Collections.Concurrent;
using LillyQuest.Engine.Interfaces.Services;
using Serilog;

namespace LillyQuest.Engine.Services;

/// <summary>
/// Dispatches work to the main thread for safe window/UI operations.
/// </summary>
public sealed class MainThreadDispatcher : IMainThreadDispatcher
{
    private sealed class WorkItem
    {
        public WorkItem(Func<object?> execute, TaskCompletionSource<object?>? completion)
        {
            Execute = execute;
            Completion = completion;
        }

        public Func<object?> Execute { get; }
        public TaskCompletionSource<object?>? Completion { get; }
    }

    private readonly ConcurrentQueue<WorkItem> _queue = new();
    private readonly ILogger _logger = Log.ForContext<MainThreadDispatcher>();

    public int MainThreadId { get; } = Environment.CurrentManagedThreadId;
    public bool IsMainThread => Environment.CurrentManagedThreadId == MainThreadId;
    public int MaxActionsPerFrame { get; set; } = 100;

    public int ExecutePending(int? maxActions = null)
    {
        var limit = maxActions ?? MaxActionsPerFrame;

        if (limit <= 0)
        {
            return 0;
        }

        var executed = 0;

        while (executed < limit && _queue.TryDequeue(out var item))
        {
            try
            {
                var result = item.Execute();
                item.Completion?.TrySetResult(result);
            }
            catch (Exception ex)
            {
                if (item.Completion != null)
                {
                    item.Completion.TrySetException(ex);
                }
                else
                {
                    _logger.Error(ex, "Main thread dispatcher action failed.");
                }
            }

            executed++;
        }

        return executed;
    }

    public void Invoke(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (IsMainThread)
        {
            action();

            return;
        }

        Invoke(
            () =>
            {
                action();

                return true;
            }
        );
    }

    public T Invoke<T>(Func<T> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        if (IsMainThread)
        {
            return func();
        }

        var completion = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        _queue.Enqueue(new(() => func(), completion));

        var result = completion.Task.GetAwaiter().GetResult();

        return result is T typed
                   ? typed
                   : throw new InvalidOperationException("Main thread invocation returned unexpected result.");
    }

    public void Post(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _queue.Enqueue(
            new(
                () =>
                {
                    action();

                    return null;
                },
                null
            )
        );
    }
}
