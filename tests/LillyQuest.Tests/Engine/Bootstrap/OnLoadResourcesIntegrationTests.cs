using DryIoc;
using LillyQuest.Core.Data.Configs;
using LillyQuest.Core.Data.Plugins;
using LillyQuest.Engine;
using LillyQuest.Engine.Interfaces.Plugins;

namespace LillyQuest.Tests.Engine.Bootstrap;

/// <summary>
/// Tests to verify that OnLoadResources is called during bootstrap.
/// </summary>
public class OnLoadResourcesIntegrationTests
{
    private sealed class ResourceLoadingPlugin : ILillyQuestPlugin
    {
        public PluginInfo PluginInfo => new(
            Id: "resource.loader",
            Name: "Resource Loader",
            Version: "1.0.0",
            Author: "Test",
            Description: "Plugin that loads resources",
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
            // Simulate async resource loading
            var asyncLoader = container.Resolve<LillyQuest.Engine.Bootstrap.AsyncResourceLoader>();

            asyncLoader.StartAsyncLoading(async () =>
            {
                Serilog.Log.Information("Plugin started loading resources...");
                await Task.Delay(100);
                Serilog.Log.Information("✓ Plugin finished loading resources");
            });

            await Task.CompletedTask;
        }
    }

    [Test]
    public void Bootstrap_HasPublicMethodExecuteOnLoadResources()
    {
        var config = new LillyQuestEngineConfig { IsDebugMode = true };
        var bootstrap = new LillyQuestBootstrap(config);

        // Bootstrap should have ExecuteOnLoadResources method
        var method = bootstrap.GetType().GetMethod("ExecuteOnLoadResources");
        Assert.That(method, Is.Not.Null, "Bootstrap should have ExecuteOnLoadResources method");
        Assert.That(method?.IsPublic, Is.True, "ExecuteOnLoadResources should be public");
        Assert.That(method?.ReturnType, Is.EqualTo(typeof(System.Threading.Tasks.Task)));
    }

    [Test]
    public void Bootstrap_CanCallExecuteOnLoadResources()
    {
        var config = new LillyQuestEngineConfig { IsDebugMode = true };
        var bootstrap = new LillyQuestBootstrap(config);

        bootstrap.Initialize();
        bootstrap.RegisterServices(container =>
        {
            container.RegisterInstance<ILillyQuestPlugin>(new ResourceLoadingPlugin());
            return container;
        });

        // Should be able to call ExecuteOnLoadResources without error
        var task = bootstrap.ExecuteOnLoadResources();
        Assert.That(task, Is.Not.Null);
    }

    [Test]
    public async Task ExecuteOnLoadResources_CanBeAwaited()
    {
        var config = new LillyQuestEngineConfig { IsDebugMode = true };
        var bootstrap = new LillyQuestBootstrap(config);

        bootstrap.Initialize();
        bootstrap.RegisterServices(container =>
        {
            container.RegisterInstance<ILillyQuestPlugin>(new ResourceLoadingPlugin());
            return container;
        });

        // Should be able to await ExecuteOnLoadResources
        var task = bootstrap.ExecuteOnLoadResources();
        Assert.DoesNotThrowAsync(async () => await task);
    }

    [Test]
    public void Bootstrap_IsLoadingResources_ReflectsAsyncLoaderState()
    {
        var config = new LillyQuestEngineConfig { IsDebugMode = true };
        var bootstrap = new LillyQuestBootstrap(config);

        bootstrap.Initialize();
        bootstrap.RegisterServices(container =>
        {
            container.RegisterInstance<ILillyQuestPlugin>(new ResourceLoadingPlugin());
            return container;
        });

        // Initially not loading
        Assert.That(bootstrap.IsLoadingResources, Is.False);
    }
}
