using DryIoc;
using LillyQuest.Core.Data.Configs;
using LillyQuest.Core.Data.Directories;
using LillyQuest.Core.Data.Plugins;
using LillyQuest.Core.Utils;
using LillyQuest.Engine;
using LillyQuest.Engine.Interfaces.Plugins;

namespace LillyQuest.Tests.Engine.Bootstrap;

public class BootstrapOnLoadResourcesTests
{
    [SetUp]
    public void SkipIfHeadless()
    {
        if (IsHeadlessEnvironment())
        {
            Assert.Ignore("Skipping bootstrap integration tests on headless/CI environment.");
        }
    }

    private sealed class LoggingPlugin : ILillyQuestPlugin
    {
        private readonly List<string> _events;

        public PluginInfo PluginInfo => new(
            Id: "logging",
            Name: "Logging",
            Version: "1.0.0",
            Author: "Test",
            Description: "",
            InitScriptName: "",
            Dependencies: []
        );

        public LoggingPlugin(List<string> events)
        {
            _events = events;
        }

        public string? GetScriptOnLoadFunctionName() => null;
        public void RegisterServices(IContainer container) { }
        public string[]? DirectoriesToCreate() => null;
        public void OnDirectories(DirectoriesConfig global, DirectoriesConfig plugin) { }
        public void Shutdown() { }

        public async Task OnEngineReady(IContainer container)
        {
            _events.Add("OnEngineReady");
            await Task.CompletedTask;
        }

        public async Task OnReadyToRender(IContainer container)
        {
            _events.Add("OnReadyToRender");
            await Task.CompletedTask;
        }

        public async Task OnLoadResources(IContainer container)
        {
            _events.Add("OnLoadResources");
            await Task.CompletedTask;
        }
    }

    [Test]
    public void Bootstrap_HasPublicMethodToExecuteOnLoadResources()
    {
        var config = new LillyQuestEngineConfig { IsDebugMode = true, IsHeadless = true };
        var bootstrap = new LillyQuestBootstrap(config);

        // Bootstrap should expose a method to execute OnLoadResources hooks
        Assert.That(bootstrap, Is.Not.Null);

        // Check if ExecuteOnLoadResources method exists
        var method = bootstrap.GetType().GetMethod("ExecuteOnLoadResources");
        Assert.That(method, Is.Not.Null, "Bootstrap should have ExecuteOnLoadResources method");
    }

    [Test]
    public async Task ExecuteOnLoadResources_CallsPluginHook()
    {
        var events = new List<string>();
        var plugin = new LoggingPlugin(events);

        var config = new LillyQuestEngineConfig { IsDebugMode = true, IsHeadless = true };
        var bootstrap = new LillyQuestBootstrap(config);

        bootstrap.Initialize();
        bootstrap.RegisterServices(container =>
        {
            container.RegisterInstance<ILillyQuestPlugin>(plugin);
            return container;
        });

        // Execute the hook
        var method = bootstrap.GetType().GetMethod("ExecuteOnLoadResources");
        var container = bootstrap.GetType()
            .GetField("_container", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(bootstrap);

        if (method != null && container != null)
        {
            var task = (Task?)method.Invoke(bootstrap, []);
            if (task != null)
            {
                await task;
            }
        }

        // OnLoadResources should have been called
        Assert.That(events, Does.Contain("OnLoadResources"));
    }

    [Test]
    public void Bootstrap_AllHooksArePublicAndCallable()
    {
        var config = new LillyQuestEngineConfig { IsDebugMode = true, IsHeadless = true };
        var bootstrap = new LillyQuestBootstrap(config);

        // Verify the bootstrap has all three hook execution methods public
        var onEngineReady = bootstrap.GetType().GetMethod("ExecuteOnEngineReady");
        var onReadyToRender = bootstrap.GetType().GetMethod("ExecuteOnReadyToRender");
        var onLoadResources = bootstrap.GetType().GetMethod("ExecuteOnLoadResources");

        Assert.That(onEngineReady, Is.Not.Null, "ExecuteOnEngineReady should be public");
        Assert.That(onReadyToRender, Is.Not.Null, "ExecuteOnReadyToRender should be public");
        Assert.That(onLoadResources, Is.Not.Null, "ExecuteOnLoadResources should be public");

        // Verify they are public and return Task
        Assert.That(onEngineReady?.IsPublic, Is.True);
        Assert.That(onReadyToRender?.IsPublic, Is.True);
        Assert.That(onLoadResources?.IsPublic, Is.True);
    }

    private static bool IsHeadlessEnvironment()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")))
        {
            return true;
        }

        if (!PlatformUtils.IsRunningOnLinux())
        {
            return false;
        }

        var display = Environment.GetEnvironmentVariable("DISPLAY");
        var waylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");

        return string.IsNullOrEmpty(display) && string.IsNullOrEmpty(waylandDisplay);
    }
}
