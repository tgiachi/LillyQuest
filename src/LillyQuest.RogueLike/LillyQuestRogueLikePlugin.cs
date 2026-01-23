using DryIoc;
using LillyQuest.Core.Data.Directories;
using LillyQuest.Core.Data.Plugins;
using LillyQuest.Engine.Interfaces.Plugins;

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

        var directoriesConfig = container.Resolve<DirectoriesConfig>();

    }
    public void Shutdown() { }
}
