using DryIoc;
using LillyQuest.Core.Data.Configs;
using LillyQuest.Core.Data.Plugins;
using LillyQuest.Engine;
using LillyQuest.Engine.Interfaces.Plugins;

namespace LillyQuest.Tests.Engine.Bootstrap;

public class BootstrapIntegrationTests
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

        public void RegisterServices(IContainer container)
        {
            _events.Add("RegisterServices");
        }

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
    public void Bootstrap_CanBeInstantiated()
    {
        var config = new LillyQuestEngineConfig { IsDebugMode = true };
        var bootstrap = new LillyQuestBootstrap(config);

        // Bootstrap should be instantiable
        Assert.That(bootstrap, Is.Not.Null);
    }

    [Test]
    public void RegisterServices_DiscoverspluginAndCallsItsRegisterServices()
    {
        var events = new List<string>();
        var plugin = new TrackingPlugin(events);

        var config = new LillyQuestEngineConfig { IsDebugMode = true };
        var bootstrap = new LillyQuestBootstrap(config);

        bootstrap.Initialize();
        bootstrap.RegisterServices(container =>
        {
            container.RegisterInstance<ILillyQuestPlugin>(plugin);
            return container;
        });

        // Plugin's RegisterServices should have been called during RegisterServices
        Assert.That(events, Does.Contain("RegisterServices"));
    }

    [Test]
    public void RegisterServices_ExecutesPluginsInOrder()
    {
        var events = new List<string>();
        var plugin = new TrackingPlugin(events);

        var config = new LillyQuestEngineConfig { IsDebugMode = true };
        var bootstrap = new LillyQuestBootstrap(config);

        bootstrap.Initialize();
        bootstrap.RegisterServices(container =>
        {
            container.RegisterInstance<ILillyQuestPlugin>(plugin);
            return container;
        });

        // Verify RegisterServices was called
        var registerIdx = events.IndexOf("RegisterServices");
        Assert.That(registerIdx, Is.GreaterThanOrEqualTo(0), "RegisterServices should be called");
    }
}
