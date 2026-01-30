using DryIoc;
using LillyQuest.Core.Data.Directories;
using LillyQuest.Core.Data.Plugins;
using LillyQuest.Engine.Bootstrap;
using LillyQuest.Engine.Interfaces.Plugins;

namespace LillyQuest.Tests.Engine.Bootstrap;

public class PluginLifecycleExecutorTests
{
    private static readonly string[] ExpectedOnEngineReady =
    [
        "tracking:OnEngineReady",
        "tracking:OnEngineReady"
    ];

    private static readonly string[] ExpectedOnReadyToRender =
    [
        "tracking:OnReadyToRender",
        "tracking:OnReadyToRender"
    ];

    private static readonly string[] ExpectedOnLoadResources =
    [
        "tracking:OnLoadResources",
        "tracking:OnLoadResources"
    ];

    private sealed class TrackingPlugin : ILillyQuestPlugin
    {
        private readonly List<string> _events;

        public PluginInfo PluginInfo
            => new(
                "tracking",
                "Tracking",
                "1.0.0",
                "Test",
                "",
                "",
                []
            );

        public TrackingPlugin(List<string> events)
            => _events = events;

        public string[]? DirectoriesToCreate()
            => null;

        public string? GetScriptOnLoadFunctionName()
            => null;

        public void OnDirectories(DirectoriesConfig globalConfig, DirectoriesConfig plugin) { }

        public async Task OnEngineReady(IContainer container)
        {
            _events.Add($"{PluginInfo.Id}:OnEngineReady");
            await Task.CompletedTask;
        }

        public async Task OnLoadResources(IContainer container)
        {
            _events.Add($"{PluginInfo.Id}:OnLoadResources");
            await Task.CompletedTask;
        }

        public async Task OnReadyToRender(IContainer container)
        {
            _events.Add($"{PluginInfo.Id}:OnReadyToRender");
            await Task.CompletedTask;
        }

        public void RegisterServices(IContainer container) { }
        public void Shutdown() { }
    }

    private sealed class FailingOnLoadPlugin : ILillyQuestPlugin
    {
        public PluginInfo PluginInfo
            => new(
                "failing",
                "Failing",
                "1.0.0",
                "Test",
                "",
                "",
                []
            );

        public string[]? DirectoriesToCreate()
            => null;

        public string? GetScriptOnLoadFunctionName()
            => null;

        public void OnDirectories(DirectoriesConfig globalConfig, DirectoriesConfig plugin) { }

        public async Task OnEngineReady(IContainer container)
        {
            await Task.CompletedTask;
        }

        public async Task OnLoadResources(IContainer container)
            => throw new InvalidOperationException("Load failed");

        public async Task OnReadyToRender(IContainer container)
        {
            await Task.CompletedTask;
        }

        public void RegisterServices(IContainer container) { }
        public void Shutdown() { }
    }

    private sealed class FailingOnEngineReadyPlugin : ILillyQuestPlugin
    {
        public PluginInfo PluginInfo
            => new(
                "failing_engine_ready",
                "Failing Engine Ready",
                "1.0.0",
                "Test",
                "",
                "",
                []
            );

        public string[]? DirectoriesToCreate()
            => null;

        public string? GetScriptOnLoadFunctionName()
            => null;

        public void OnDirectories(DirectoriesConfig globalConfig, DirectoriesConfig plugin) { }

        public async Task OnEngineReady(IContainer container)
            => throw new InvalidOperationException("Load failed");

        public async Task OnLoadResources(IContainer container)
        {
            await Task.CompletedTask;
        }

        public async Task OnReadyToRender(IContainer container)
        {
            await Task.CompletedTask;
        }

        public void RegisterServices(IContainer container) { }
        public void Shutdown() { }
    }

    [Test]
    public async Task ExecuteOnEngineReady_CallsAllPluginsInOrder()
    {
        var events = new List<string>();
        var plugins = new ILillyQuestPlugin[]
        {
            new TrackingPlugin(events),
            new TrackingPlugin(events)
        };

        var container = new Container();
        var executor = new PluginLifecycleExecutor(plugins);

        await executor.ExecuteOnEngineReady(container);

        Assert.That(events, Is.EqualTo(ExpectedOnEngineReady));
    }

    [Test]
    public void ExecuteOnEngineReady_FirstPluginFails_ThrowsException()
    {
        var plugins = new ILillyQuestPlugin[]
        {
            new FailingOnEngineReadyPlugin(),
            new TrackingPlugin(new())
        };

        var container = new Container();
        var executor = new PluginLifecycleExecutor(plugins);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await executor.ExecuteOnEngineReady(container));

        Assert.That(ex?.Message, Does.Contain("Load failed"));
    }

    [Test]
    public async Task ExecuteOnLoadResources_CallsAllPluginsInOrder()
    {
        var events = new List<string>();
        var plugins = new ILillyQuestPlugin[]
        {
            new TrackingPlugin(events),
            new TrackingPlugin(events)
        };

        var container = new Container();
        var executor = new PluginLifecycleExecutor(plugins);

        await executor.ExecuteOnLoadResources(container);

        Assert.That(events, Is.EqualTo(ExpectedOnLoadResources));
    }

    [Test]
    public void ExecuteOnLoadResources_FirstPluginFails_ThrowsException()
    {
        var plugins = new ILillyQuestPlugin[]
        {
            new FailingOnLoadPlugin(),
            new TrackingPlugin(new())
        };

        var container = new Container();
        var executor = new PluginLifecycleExecutor(plugins);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await executor.ExecuteOnLoadResources(container));

        Assert.That(ex?.Message, Does.Contain("Load failed"));
    }

    [Test]
    public async Task ExecuteOnReadyToRender_CallsAllPluginsInOrder()
    {
        var events = new List<string>();
        var plugins = new ILillyQuestPlugin[]
        {
            new TrackingPlugin(events),
            new TrackingPlugin(events)
        };

        var container = new Container();
        var executor = new PluginLifecycleExecutor(plugins);

        await executor.ExecuteOnReadyToRender(container);

        Assert.That(events, Is.EqualTo(ExpectedOnReadyToRender));
    }
}
