namespace LillyQuest.Engine.Bootstrap;

public sealed class ResourceLoadingFlow
{
    private readonly IAsyncResourceLoader _loader;
    private bool _started;
    private bool _completed;
    private Func<Task>? _onLoadLua;
    private Func<Task>? _onReadyToRender;
    private Func<Task>? _onLoadResources;
    private Action? _onLoadingComplete;
    private Task? _luaTask;
    private Task? _readyTask;
    private Task? _loadTask;

    public ResourceLoadingFlow(IAsyncResourceLoader loader)
        => _loader = loader ?? throw new ArgumentNullException(nameof(loader));

    public void StartLoading(
        Func<Task> onLoadLua,
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
        _onLoadLua = onLoadLua;
        _onReadyToRender = onReadyToRender;
        _onLoadResources = onLoadResources;
        _onLoadingComplete = onLoadingComplete;
        showLogScreen();
        _luaTask = _onLoadLua();
    }

    public void Update()
    {
        if (!_started || _completed)
        {
            return;
        }

        if (_luaTask == null || _onReadyToRender == null || _onLoadResources == null)
        {
            return;
        }

        if (!_luaTask.IsCompleted)
        {
            return;
        }

        if (_luaTask.IsFaulted)
        {
            _luaTask.GetAwaiter().GetResult();
        }

        if (_readyTask == null)
        {
            _readyTask = _onReadyToRender();
        }

        if (!_readyTask.IsCompleted)
        {
            return;
        }

        if (_readyTask.IsFaulted)
        {
            _readyTask.GetAwaiter().GetResult();
        }

        if (_loadTask == null)
        {
            _loadTask = _onLoadResources();
        }

        if (!_loadTask.IsCompleted)
        {
            return;
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
