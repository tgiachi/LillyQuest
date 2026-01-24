using DryIoc;
using LillyQuest.Core.Data.Configs;
using LillyQuest.Core.Data.Directories;
using LillyQuest.Core.Data.Plugins;
using LillyQuest.Engine;
using LillyQuest.Engine.Interfaces.Plugins;

namespace LillyQuest.Tests.Engine.Bootstrap;

public class BootstrapOnReadyToRenderTests
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
    public void Bootstrap_HasPublicMethodToExecuteOnReadyToRender()
    {
        var config = new LillyQuestEngineConfig { IsDebugMode = true };
        var bootstrap = new LillyQuestBootstrap(config);

        // Bootstrap should expose a method to execute OnReadyToRender hooks
        Assert.That(bootstrap, Is.Not.Null);

        // Check if ExecuteOnReadyToRender method exists (it should be public for testing)
        var method = bootstrap.GetType().GetMethod("ExecuteOnReadyToRender");
        Assert.That(method, Is.Not.Null, "Bootstrap should have ExecuteOnReadyToRender method");
    }

    [Test]
    public void Bootstrap_OnReadyToRenderIsPublic()
    {
        var config = new LillyQuestEngineConfig { IsDebugMode = true };
        var bootstrap = new LillyQuestBootstrap(config);

        // ExecuteOnReadyToRender should be public
        var method = bootstrap.GetType().GetMethod("ExecuteOnReadyToRender");
        Assert.That(method, Is.Not.Null);
        Assert.That(method?.IsPublic, Is.True, "ExecuteOnReadyToRender should be public");
    }

    [Test]
    public void Bootstrap_OnReadyToRenderReturnsTask()
    {
        var config = new LillyQuestEngineConfig { IsDebugMode = true };
        var bootstrap = new LillyQuestBootstrap(config);

        // ExecuteOnReadyToRender should return Task
        var method = bootstrap.GetType().GetMethod("ExecuteOnReadyToRender");
        Assert.That(method?.ReturnType, Is.EqualTo(typeof(Task)), "ExecuteOnReadyToRender should return Task");
    }
}
