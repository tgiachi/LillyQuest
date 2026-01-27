using DryIoc;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Data.Directories;
using LillyQuest.Core.Data.Plugins;
using LillyQuest.Core.Json;
using LillyQuest.Engine.Interfaces.Plugins;
using LillyQuest.RogueLike.Data;
using LillyQuest.RogueLike.Data.Configs;
using LillyQuest.RogueLike.Interfaces;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Json.Context;
using LillyQuest.RogueLike.Services;
using LillyQuest.RogueLike.Services.Loader;
using Serilog;

namespace LillyQuest.RogueLike;

public class LillyQuestRogueLikePlugin : ILillyQuestPlugin
{
    private readonly ILogger _logger = Log.ForContext<LillyQuestRogueLikePlugin>();

    private IContainer _container;

    private readonly List<Type> _dataReceiverTypes =
    [
        typeof(ColorService),
        typeof(TileSetService),
        typeof(TerrainService)
    ];

    public PluginInfo PluginInfo
        => new PluginInfo(
            "com.github.tgiachi.lillyquest.roguelike",
            "LillyQuest RogueLike Plugin",
            "0.5.0",
            "squid",
            "Adds RogueLike specific features to LillyQuest Engine",
            "roguelike.lua",
            []
        );

    public string? GetScriptOnLoadFunctionName()
        => string.Empty;

    public void RegisterServices(IContainer container)
    {
        JsonUtils.RegisterJsonContext(LillyQuestRogueLikeJsonContext.Default);

        _container = container;

        container.Register<IDataLoaderService, DataLoaderService>();
        container.Register<IMapGenerator, MapGenerator>();

        foreach (var receiverType in _dataReceiverTypes)
        {
            _logger.Debug("Registering receiver type: {ReceiverType}", receiverType);

            _container.Register(receiverType, Reuse.Singleton);
        }
    }

    public string[] DirectoriesToCreate()
        => ["data"];

    public void OnDirectories(DirectoriesConfig global, DirectoriesConfig plugin)
    {
        _container.RegisterInstance(
            new DataLoaderConfig
            {
                PluginDirectory = plugin
            }
        );

        _logger.Information("Plugin Directories Configuration: {Root}", plugin.Root);
    }

    public void Shutdown() { }

    public async Task OnReadyToRender(IContainer container) { }

    public async Task OnLoadResources(IContainer container)
    {
        Log.Information("Loading RogueLike plugin...");
        await Task.Delay(1000);

        var dataLoader = container.Resolve<IDataLoaderService>();

        foreach (var receiverType in _dataReceiverTypes)
        {
            var receiver = (IDataLoaderReceiver)container.Resolve(receiverType);
            dataLoader.RegisterDataReceiver(receiver);
        }

        _logger.Information("Loading RogueLike data");

        await dataLoader.LoadDataAsync();

        _logger.Information("RogueLike data loaded");

        await dataLoader.DispatchDataToReceiversAsync();

        _logger.Information("Starting data verification");
        await dataLoader.VerifyLoadedDataAsync();


        var mapGenerator = container.Resolve<IMapGenerator>();
        await mapGenerator.GenerateMapAsync();

        // var renderContext = container.Resolve<EngineRenderContext>();
        //
        // for (var progress = 0; progress <= 100; progress += Random.Shared.Next(0, 4))
        // {
        //     Log.Information("\rRogueLike loading {Progress}%", progress);
        //     await Task.Delay(100);
        // }
        //
        // renderContext.PostOnMainThread(
        //     () =>
        //     {
        //         renderContext.Window.Title = "LillyQuest RogueLike";
        //     }
        // );
        //
        // Log.Information("RogueLike plugin loaded");
        //
        // await Task.Delay(1000);
        // Log.Information("\n");
        // await Task.Delay(1000);
    }
}
