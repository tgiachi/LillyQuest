namespace LillyQuest.Engine.Bootstrap;

public sealed class ResourceLoadingFlow
{
    private readonly IAsyncResourceLoader _loader;
    private bool _started;
    private bool _completed;
    private Action? _onLoadingComplete;
    private Task? _readyTask;
    private Task? _loadTask;

    public ResourceLoadingFlow(IAsyncResourceLoader loader)
        => _loader = loader ?? throw new ArgumentNullException(nameof(loader));

    public void StartLoading(
        Func<Task> onReadyToRender,
        Func<Task> onLoadResources,
        Action showLogScreen,
        Action onLoadingComplete
    )
    {
        if (_started)
        {
            return;
        }

        _started = true;
        _onLoadingComplete = onLoadingComplete;
        showLogScreen();
        _readyTask = onReadyToRender();
        _loadTask = onLoadResources();
    }

    public void Update()
    {
        if (!_started || _completed)
        {
            return;
        }

        if (_readyTask == null || _loadTask == null)
        {
            return;
        }

        if (!_readyTask.IsCompleted || !_loadTask.IsCompleted)
        {
            return;
        }

        if (_readyTask.IsFaulted)
        {
            _readyTask.GetAwaiter().GetResult();
        }

        if (_loadTask.IsFaulted)
        {
            _loadTask.GetAwaiter().GetResult();
        }

        if (!_loader.IsLoadingComplete)
        {
            return;
        }

        _completed = true;
        _onLoadingComplete?.Invoke();
    }
}
