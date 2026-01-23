using DryIoc;
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

    public void Shutdown() { }

    public  async Task OnReadyToRender(IContainer container)
    {



    }

    public async  Task OnLoadResources(IContainer container)
    {

        foreach (var i in Enumerable.Range(1, 14))
        {
            Log.Information("Loading RogueLike assets... {Step}/14", i);


            await Task.Delay(500);
        }
        await Task.Delay(1000);

        Log.Information("Loading RogueLike plugin...");

        await Task.Delay(1000);
    }
}
