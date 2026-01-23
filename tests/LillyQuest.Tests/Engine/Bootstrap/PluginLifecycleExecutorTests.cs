using DryIoc;
using LillyQuest.Core.Data.Plugins;
using LillyQuest.Engine.Bootstrap;
using LillyQuest.Engine.Interfaces.Plugins;

namespace LillyQuest.Tests.Engine.Bootstrap;

public class PluginLifecycleExecutorTests
{
    private sealed class TrackingPlugin : ILillyQuestPlugin
    {
        private readonly List<string> _events;

        public PluginInfo PluginInfo => new(
            Id: "tracking",
            Name: "Tracking",
            Version: "1.0.0",
            Author: "Test",
            Description: "",
            Dependencies: []
        );

        public TrackingPlugin(List<string> events)
        {
            _events = events;
        }

        public string? GetScriptOnLoadFunctionName() => null;
        public void RegisterServices(IContainer container) { }
        public void Shutdown() { }

        public async Task OnEngineReady(IContainer container)
        {
            _events.Add($"{PluginInfo.Id}:OnEngineReady");
            await Task.CompletedTask;
        }

        public async Task OnReadyToRender(IContainer container)
        {
            _events.Add($"{PluginInfo.Id}:OnReadyToRender");
            await Task.CompletedTask;
        }

        public async Task OnLoadResources(IContainer container)
        {
            _events.Add($"{PluginInfo.Id}:OnLoadResources");
            await Task.CompletedTask;
        }
    }

    private sealed class FailingOnLoadPlugin : ILillyQuestPlugin
    {
        public PluginInfo PluginInfo => new(
            Id: "failing",
            Name: "Failing",
            Version: "1.0.0",
            Author: "Test",
            Description: "",
            Dependencies: []
        );

        public string? GetScriptOnLoadFunctionName() => null;
        public void RegisterServices(IContainer container) { }
        public void Shutdown() { }

        public async Task OnEngineReady(IContainer container)
        {
            await Task.CompletedTask;
        }

        public async Task OnReadyToRender(IContainer container)
        {
            await Task.CompletedTask;
        }

        public async Task OnLoadResources(IContainer container)
        {
            throw new InvalidOperationException("Load failed");
        }
    }

    private sealed class FailingOnEngineReadyPlugin : ILillyQuestPlugin
    {
        public PluginInfo PluginInfo => new(
            Id: "failing_engine_ready",
            Name: "Failing Engine Ready",
            Version: "1.0.0",
            Author: "Test",
            Description: "",
            Dependencies: []
        );

        public string? GetScriptOnLoadFunctionName() => null;
        public void RegisterServices(IContainer container) { }
        public void Shutdown() { }

        public async Task OnEngineReady(IContainer container)
        {
            throw new InvalidOperationException("Load failed");
        }

        public async Task OnReadyToRender(IContainer container)
        {
            await Task.CompletedTask;
        }

        public async Task OnLoadResources(IContainer container)
        {
            await Task.CompletedTask;
        }
    }

    [Test]
    public async Task ExecuteOnEngineReady_CallsAllPluginsInOrder()
    {
        var events = new List<string>();
        var plugins = new ILillyQuestPlugin[]
        {
            new TrackingPlugin(events),
            new TrackingPlugin(events),
        };

        var container = new Container();
        var executor = new PluginLifecycleExecutor(plugins);

        await executor.ExecuteOnEngineReady(container);

        Assert.That(events, Is.EqualTo(new[] { "tracking:OnEngineReady", "tracking:OnEngineReady" }));
    }

    [Test]
    public async Task ExecuteOnReadyToRender_CallsAllPluginsInOrder()
    {
        var events = new List<string>();
        var plugins = new ILillyQuestPlugin[]
        {
            new TrackingPlugin(events),
            new TrackingPlugin(events),
        };

        var container = new Container();
        var executor = new PluginLifecycleExecutor(plugins);

        await executor.ExecuteOnReadyToRender(container);

        Assert.That(events, Is.EqualTo(new[] { "tracking:OnReadyToRender", "tracking:OnReadyToRender" }));
    }

    [Test]
    public async Task ExecuteOnLoadResources_CallsAllPluginsInOrder()
    {
        var events = new List<string>();
        var plugins = new ILillyQuestPlugin[]
        {
            new TrackingPlugin(events),
            new TrackingPlugin(events),
        };

        var container = new Container();
        var executor = new PluginLifecycleExecutor(plugins);

        await executor.ExecuteOnLoadResources(container);

        Assert.That(events, Is.EqualTo(new[] { "tracking:OnLoadResources", "tracking:OnLoadResources" }));
    }

    [Test]
    public void ExecuteOnLoadResources_FirstPluginFails_ThrowsException()
    {
        var plugins = new ILillyQuestPlugin[]
        {
            new FailingOnLoadPlugin(),
            new TrackingPlugin(new()),
        };

        var container = new Container();
        var executor = new PluginLifecycleExecutor(plugins);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await executor.ExecuteOnLoadResources(container)
        );

        Assert.That(ex?.Message, Does.Contain("Load failed"));
    }

    [Test]
    public void ExecuteOnEngineReady_FirstPluginFails_ThrowsException()
    {
        var plugins = new ILillyQuestPlugin[]
        {
            new FailingOnEngineReadyPlugin(),
            new TrackingPlugin(new()),
        };

        var container = new Container();
        var executor = new PluginLifecycleExecutor(plugins);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await executor.ExecuteOnEngineReady(container)
        );

        Assert.That(ex?.Message, Does.Contain("Load failed"));
    }
}
