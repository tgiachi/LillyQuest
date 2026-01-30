using System.Collections.Concurrent;
using LillyQuest.Engine.Interfaces.Services;

namespace LillyQuest.Engine.Services;

public sealed class JobScheduler : IJobScheduler, IDisposable
{
    private readonly ConcurrentQueue<Func<Task>> _jobs = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly List<Task> _workers = new();
    private CancellationTokenSource? _cts;
    private bool _running;
    private bool _stopped;

    public void Start(int workerCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(workerCount);

        if (_running)
        {
            return;
        }

        if (_stopped)
        {
            throw new InvalidOperationException("Job scheduler has been stopped.");
        }

        _cts = new();
        _running = true;

        for (var i = 0; i < workerCount; i++)
        {
            _workers.Add(Task.Run(() => WorkerLoop(_cts.Token), _cts.Token));
        }
    }

    public void Enqueue(Action job)
    {
        ArgumentNullException.ThrowIfNull(job);

        Enqueue(() =>
                {
                    job();
                    return Task.CompletedTask;
                }
        );
    }

    public void Enqueue(Func<Task> job)
    {
        ArgumentNullException.ThrowIfNull(job);

        if (_stopped)
        {
            throw new InvalidOperationException("Job scheduler has been stopped.");
        }

        _jobs.Enqueue(job);
        _signal.Release();
    }

    public async Task StopAsync()
    {
        if (_stopped)
        {
            return;
        }

        _stopped = true;
        _running = false;

        if (_cts == null)
        {
            return;
        }

        _cts.Cancel();

        for (var i = 0; i < _workers.Count; i++)
        {
            _signal.Release();
        }

        try
        {
            await Task.WhenAll(_workers);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
        _cts?.Dispose();
        _signal.Dispose();
    }

    private async Task WorkerLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _signal.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (!_jobs.TryDequeue(out var job))
            {
                continue;
            }

            try
            {
                await job();
            }
            catch
            {
                // Swallow job exceptions to keep worker alive.
            }
        }
    }
}
