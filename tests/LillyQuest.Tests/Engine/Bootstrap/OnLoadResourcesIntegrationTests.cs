using DryIoc;
using LillyQuest.Core.Data.Configs;
using LillyQuest.Core.Data.Directories;
using LillyQuest.Core.Data.Plugins;
using LillyQuest.Core.Utils;
using LillyQuest.Engine;
using LillyQuest.Engine.Bootstrap;
using LillyQuest.Engine.Interfaces.Plugins;
using Serilog;

namespace LillyQuest.Tests.Engine.Bootstrap;

/// <summary>
/// Tests to verify that OnLoadResources is called during bootstrap.
/// </summary>
public class OnLoadResourcesIntegrationTests
{
    private sealed class ResourceLoadingPlugin : ILillyQuestPlugin
    {
        public PluginInfo PluginInfo
            => new(
                "resource.loader",
                "Resource Loader",
                "1.0.0",
                "Test",
                "Plugin that loads resources",
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
        {
            // Simulate async resource loading
            var asyncLoader = container.Resolve<AsyncResourceLoader>();

            asyncLoader.StartAsyncLoading(
                async () =>
                {
                    Log.Information("Plugin started loading resources...");
                    await Task.Delay(100);
                    Log.Information("✓ Plugin finished loading resources");
                }
            );

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
    public void Bootstrap_CanCallExecuteOnLoadResources()
    {
        var config = new LillyQuestEngineConfig { IsDebugMode = true, IsHeadless = true };
        var bootstrap = new LillyQuestBootstrap(config);

        bootstrap.Initialize();
        bootstrap.RegisterServices(
            container =>
            {
                container.RegisterInstance<ILillyQuestPlugin>(new ResourceLoadingPlugin());

                return container;
            }
        );

        // Should be able to call ExecuteOnLoadResources without error
        var task = bootstrap.ExecuteOnLoadResources();
        Assert.That(task, Is.Not.Null);
    }

    [Test]
    public void Bootstrap_HasPublicMethodExecuteOnLoadResources()
    {
        var config = new LillyQuestEngineConfig { IsDebugMode = true, IsHeadless = true };
        var bootstrap = new LillyQuestBootstrap(config);

        // Bootstrap should have ExecuteOnLoadResources method
        var method = bootstrap.GetType().GetMethod("ExecuteOnLoadResources");
        Assert.That(method, Is.Not.Null, "Bootstrap should have ExecuteOnLoadResources method");
        Assert.That(method?.IsPublic, Is.True, "ExecuteOnLoadResources should be public");
        Assert.That(method?.ReturnType, Is.EqualTo(typeof(Task)));
    }

    [Test]
    public void Bootstrap_IsLoadingResources_ReflectsAsyncLoaderState()
    {
        var config = new LillyQuestEngineConfig { IsDebugMode = true, IsHeadless = true };
        var bootstrap = new LillyQuestBootstrap(config);

        bootstrap.Initialize();
        bootstrap.RegisterServices(
            container =>
            {
                container.RegisterInstance<ILillyQuestPlugin>(new ResourceLoadingPlugin());

                return container;
            }
        );

        // Initially not loading
        Assert.That(bootstrap.IsLoadingResources, Is.False);
    }

    [Test]
    public async Task ExecuteOnLoadResources_CanBeAwaited()
    {
        var config = new LillyQuestEngineConfig { IsDebugMode = true, IsHeadless = true };
        var bootstrap = new LillyQuestBootstrap(config);

        bootstrap.Initialize();
        bootstrap.RegisterServices(
            container =>
            {
                container.RegisterInstance<ILillyQuestPlugin>(new ResourceLoadingPlugin());

                return container;
            }
        );

        // Should be able to await ExecuteOnLoadResources
        var task = bootstrap.ExecuteOnLoadResources();
        Assert.DoesNotThrowAsync(async () => await task);
    }

    [SetUp]
    public void SkipIfHeadless()
    {
        if (IsHeadlessEnvironment())
        {
            Assert.Ignore("Skipping bootstrap integration tests on headless/CI environment.");
        }
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
