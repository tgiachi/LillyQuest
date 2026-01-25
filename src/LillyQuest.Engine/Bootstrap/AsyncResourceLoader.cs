using DryIoc;
using Serilog;

namespace LillyQuest.Engine.Bootstrap;

/// <summary>
/// Manages asynchronous resource loading without blocking the render loop.
/// Allows plugins to load resources while the game continues rendering.
/// </summary>
public sealed class AsyncResourceLoader : IAsyncResourceLoader
{
    private readonly ILogger _logger = Log.ForContext<AsyncResourceLoader>();
    private Task? _loadingTask;

    /// <summary>
    /// Gets whether resources are currently being loaded.
    /// </summary>
    public bool IsLoading { get; private set; }

    /// <summary>
    /// Starts an async resource loading operation without blocking the render loop.
    /// The operation runs in a background task while the engine continues rendering.
    /// </summary>
    /// <param name="loadingOperation">The async operation to execute in background.</param>
    public void StartAsyncLoading(Func<Task> loadingOperation)
    {
        if (IsLoading)
        {
            _logger.Warning("A loading operation is already in progress");

            return;
        }

        IsLoading = true;
        _loadingTask = Task.Run(
            async () =>
            {
                try
                {
                    _logger.Information("Starting async resource loading...");
                    await loadingOperation();
                    _logger.Information("✓ Async resource loading completed");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "✗ Async resource loading failed");

                    throw;
                }
                finally
                {
                    IsLoading = false;
                }
            }
        );
    }

    /// <summary>
    /// Waits for the current loading operation to complete.
    /// Safe to call even if no loading is in progress.
    /// </summary>
    public async Task WaitForLoadingComplete()
    {
        if (_loadingTask != null)
        {
            await _loadingTask;
        }
    }

    /// <summary>
    /// Checks if loading is complete. Non-blocking.
    /// </summary>
    public bool IsLoadingComplete => _loadingTask?.IsCompleted ?? true;
}
