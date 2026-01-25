using DryIoc;
using LillyQuest.Core.Data.Directories;
using LillyQuest.Core.Data.Plugins;
using LillyQuest.Engine.Interfaces.Plugins;

namespace LillyQuest.Tests.Engine.Bootstrap;

public class PluginLifecycleTests
{
    private sealed class TestPlugin : ILillyQuestPlugin
    {
        private readonly List<string> _executedHooks;

        public PluginInfo PluginInfo => new(
            Id: "test.plugin",
            Name: "Test Plugin",
            Version: "1.0.0",
            Author: "Test",
            Description: "Test plugin",
            InitScriptName: "",
            Dependencies: []
        );

        public TestPlugin(List<string> executedHooks)
        {
            _executedHooks = executedHooks;
        }

        public string? GetScriptOnLoadFunctionName() => null;

        public void RegisterServices(IContainer container) { }

        public void OnDirectories(DirectoriesConfig global, DirectoriesConfig plugin) { }

        public void Shutdown() { }

        public async Task OnEngineReady(IContainer container)
        {
            _executedHooks.Add("OnEngineReady");
            await Task.CompletedTask;
        }

        public async Task OnReadyToRender(IContainer container)
        {
            _executedHooks.Add("OnReadyToRender");
            await Task.CompletedTask;
        }

        public async Task OnLoadResources(IContainer container)
        {
            _executedHooks.Add("OnLoadResources");
            await Task.CompletedTask;
        }
    }

    private sealed class FailingPlugin : ILillyQuestPlugin
    {
        private readonly string _failOnHook;

        public PluginInfo PluginInfo => new(
            Id: "failing.plugin",
            Name: "Failing Plugin",
            Version: "1.0.0",
            Author: "Test",
            Description: "Plugin that fails on a specific hook",
            InitScriptName: "",
            Dependencies: []
        );

        public FailingPlugin(string failOnHook)
        {
            _failOnHook = failOnHook;
        }

        public string? GetScriptOnLoadFunctionName() => null;

        public void RegisterServices(IContainer container) { }

        public void OnDirectories(DirectoriesConfig global, DirectoriesConfig plugin) { }

        public void Shutdown() { }

        public async Task OnEngineReady(IContainer container)
        {
            if (_failOnHook == "OnEngineReady")
                throw new InvalidOperationException("OnEngineReady failed");
            await Task.CompletedTask;
        }

        public async Task OnReadyToRender(IContainer container)
        {
            if (_failOnHook == "OnReadyToRender")
                throw new InvalidOperationException("OnReadyToRender failed");
            await Task.CompletedTask;
        }

        public async Task OnLoadResources(IContainer container)
        {
            if (_failOnHook == "OnLoadResources")
                throw new InvalidOperationException("OnLoadResources failed");
            await Task.CompletedTask;
        }
    }

    [Test]
    public async Task ExecutePluginHook_OnEngineReady_CallsHookOnAllPlugins()
    {
        var executedHooks = new List<string>();
        var plugin1 = new TestPlugin(executedHooks);
        var plugin2 = new TestPlugin(executedHooks);
        var plugins = new[] { plugin1, plugin2 };

        var container = new Container();

        // Simulating bootstrap hook execution
        foreach (var plugin in plugins)
        {
            await plugin.OnEngineReady(container);
        }

        Assert.That(executedHooks, Is.EqualTo(new[] { "OnEngineReady", "OnEngineReady" }));
    }

    [Test]
    public async Task ExecutePluginHook_OnReadyToRender_CallsHookOnAllPlugins()
    {
        var executedHooks = new List<string>();
        var plugin1 = new TestPlugin(executedHooks);
        var plugin2 = new TestPlugin(executedHooks);
        var plugins = new[] { plugin1, plugin2 };

        var container = new Container();

        // Simulating bootstrap hook execution
        foreach (var plugin in plugins)
        {
            await plugin.OnReadyToRender(container);
        }

        Assert.That(executedHooks, Is.EqualTo(new[] { "OnReadyToRender", "OnReadyToRender" }));
    }

    [Test]
    public async Task ExecutePluginHook_OnLoadResources_CallsHookOnAllPlugins()
    {
        var executedHooks = new List<string>();
        var plugin1 = new TestPlugin(executedHooks);
        var plugin2 = new TestPlugin(executedHooks);
        var plugins = new[] { plugin1, plugin2 };

        var container = new Container();

        // Simulating bootstrap hook execution
        foreach (var plugin in plugins)
        {
            await plugin.OnLoadResources(container);
        }

        Assert.That(executedHooks, Is.EqualTo(new[] { "OnLoadResources", "OnLoadResources" }));
    }

    [Test]
    public async Task ExecutePluginHook_SequentialExecution_CallsOnEngineReadyThenOnReadyToRenderThenOnLoadResources()
    {
        var executedHooks = new List<string>();
        var plugin = new TestPlugin(executedHooks);
        var plugins = new[] { plugin };

        var container = new Container();

        // Phase 1: OnEngineReady
        foreach (var p in plugins)
            await p.OnEngineReady(container);

        // Phase 2: OnReadyToRender
        foreach (var p in plugins)
            await p.OnReadyToRender(container);

        // Phase 3: OnLoadResources
        foreach (var p in plugins)
            await p.OnLoadResources(container);

        Assert.That(executedHooks, Is.EqualTo(new[]
        {
            "OnEngineReady",
            "OnReadyToRender",
            "OnLoadResources"
        }));
    }

    [Test]
    public void ExecutePluginHook_OnEngineReadyThrows_PropagatesException()
    {
        var plugin = new FailingPlugin("OnEngineReady");
        var container = new Container();

        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await plugin.OnEngineReady(container)
        );

        Assert.That(ex?.Message, Does.Contain("OnEngineReady failed"));
    }

    [Test]
    public void ExecutePluginHook_OnReadyToRenderThrows_PropagatesException()
    {
        var plugin = new FailingPlugin("OnReadyToRender");
        var container = new Container();

        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await plugin.OnReadyToRender(container)
        );

        Assert.That(ex?.Message, Does.Contain("OnReadyToRender failed"));
    }

    [Test]
    public void ExecutePluginHook_OnLoadResourcesThrows_PropagatesException()
    {
        var plugin = new FailingPlugin("OnLoadResources");
        var container = new Container();

        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await plugin.OnLoadResources(container)
        );

        Assert.That(ex?.Message, Does.Contain("OnLoadResources failed"));
    }

    [Test]
    public async Task ExecutePluginHook_FirstPluginFails_DoesNotCallSubsequentPlugins()
    {
        var executedHooks = new List<string>();
        var failingPlugin = new FailingPlugin("OnEngineReady");
        var successPlugin = new TestPlugin(executedHooks);
        var plugins = new ILillyQuestPlugin[] { failingPlugin, successPlugin };

        var container = new Container();

        // Try to execute but first plugin fails
        try
        {
            foreach (var plugin in plugins)
            {
                await plugin.OnEngineReady(container);
            }
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        // successPlugin should not have been called since we stop on first failure
        Assert.That(executedHooks, Is.Empty);
    }
}
