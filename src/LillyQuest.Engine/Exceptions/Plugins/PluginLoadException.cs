using LillyQuest.Core.Data.Plugins;

namespace LillyQuest.Engine.Exceptions.Plugins;

/// <summary>
/// Exception thrown when a plugin fails to load or validate.
/// </summary>
public class PluginLoadException : Exception
{
    public string PluginId { get; }
    public PluginInfo PluginData { get; }
    public IReadOnlyList<PluginInfo> LoadedPlugins { get; }

    public PluginLoadException(
        string message,
        string pluginId,
        PluginInfo pluginData,
        IReadOnlyList<PluginInfo> loadedPlugins,
        Exception? innerException = null
    )
        : base(message, innerException)
    {
        PluginId = pluginId;
        PluginData = pluginData;
        LoadedPlugins = loadedPlugins;
    }

    public override string ToString()
    {
        var baseString = base.ToString();
        var loadedPluginsStr = string.Join(", ", LoadedPlugins.Select(p => $"{p.Id}@{p.Version}"));

        return $"{baseString}\n\nPlugin: {PluginId} v{PluginData.Version}\n" +
               $"Already Loaded: [{loadedPluginsStr}]";
    }
}
