using DryIoc;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Data.Directories;
using LillyQuest.Core.Data.Plugins;
using LillyQuest.Core.Json;
using LillyQuest.Engine.Interfaces.Plugins;
using LillyQuest.RogueLike.Json.Context;
using Serilog;

namespace LillyQuest.RogueLike;

public class LillyQuestRogueLikePlugin : ILillyQuestPlugin
{
    public PluginInfo PluginInfo
        => new PluginInfo(
            "com.github.tgiachi.lillyquest.roguelike",
            "LillyQuest RogueLike Plugin",
            "0.5.0",
            "squid",
            "Adds RogueLike specific features to LillyQuest Engine",
            []
        );

    public string? GetScriptOnLoadFunctionName()
        => string.Empty;

    public void RegisterServices(IContainer container)
    {
        JsonUtils.RegisterJsonContext(LillyQuestRogueLikeJsonContext.Default);

        var directoriesConfig = container.Resolve<DirectoriesConfig>();
    }

    public void OnDirectories(DirectoriesConfig global, DirectoriesConfig plugin)
    {

    }

    public void Shutdown() { }

    public async Task OnReadyToRender(IContainer container) { }

    public async Task OnLoadResources(IContainer container)
    {
        Log.Information("Loading RogueLike plugin...");
        await Task.Delay(1000);

        var renderContext = container.Resolve<EngineRenderContext>();

        for (var progress = 0; progress <= 100; progress += Random.Shared.Next(0, 4))
        {
            Log.Information("\rRogueLike loading {Progress}%", progress);
            await Task.Delay(100);
        }

        renderContext.Window.Title = "LillyQuest RogueLike";

        Log.Information("RogueLike plugin loaded");

        await Task.Delay(1000);
        Log.Information("\n");
        await Task.Delay(1000);
    }
}
