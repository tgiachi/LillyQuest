using DarkLilly.Core.Data.Plugins;
using DarkLilly.Engine.Exceptions.Plugins;
using DarkLilly.Engine.Interfaces.Plugins;

namespace DarkLilly.Engine.Services.Plugins;

/// <summary>
/// Central registry for managing plugin lifecycle and tracking loaded plugins.
/// </summary>
public class PluginRegistry
{
    private readonly List<IDarkLillyPlugin> _loadedPlugins = [];
    private readonly List<PluginInfo> _loadedPluginData = [];
    private readonly PluginDependencyValidator _dependencyValidator;
    private readonly List<string> _scriptEngineLoadFunctions = [];

    public PluginRegistry(PluginDependencyValidator? dependencyValidator = null)
        => _dependencyValidator = dependencyValidator ?? new PluginDependencyValidator();

    /// <summary>
    /// Checks if a plugin can be loaded based on dependency requirements.
    /// </summary>
    public bool CanLoad(PluginInfo pluginData)
    {
        try
        {
            _dependencyValidator.ValidateDependencies(pluginData, GetLoadedPluginData());

            return true;
        }
        catch (PluginLoadException)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks for circular dependencies in a set of plugins.
    /// </summary>
    public List<string> CheckForCircularDependencies(IReadOnlyList<PluginInfo> allPlugins)
        => _dependencyValidator.DetectCircularDependencies(allPlugins);

    /// <summary>
    /// Gets the dependency chain for a specific plugin.
    /// </summary>
    public IReadOnlyList<IDarkLillyPlugin> GetDependencyChain(string pluginId)
    {
        var plugin = GetPluginById(pluginId);
        var chain = new List<IDarkLillyPlugin> { plugin };

        if (plugin.PluginInfo.Dependencies != null)
        {
            foreach (var depId in plugin.PluginInfo.Dependencies)
            {
                var dep = GetPluginById(depId);
                chain.AddRange(GetDependencyChain(depId));
            }
        }

        return chain.Distinct().ToList();
    }

    /// <summary>
    /// Gets plugins that depend on the specified plugin.
    /// </summary>
    public IReadOnlyList<IDarkLillyPlugin> GetDependentsOf(string pluginId)
    {
        return _loadedPlugins
               .Where(p => p.PluginInfo.Dependencies?.Contains(pluginId) ?? false)
               .ToList()
               .AsReadOnly();
    }

    /// <summary>
    /// Gets metadata for all loaded plugins.
    /// </summary>
    public IReadOnlyList<PluginInfo> GetLoadedPluginData()
        => _loadedPluginData.AsReadOnly();

    /// <summary>
    /// Gets all currently loaded plugins.
    /// </summary>
    public IReadOnlyList<IDarkLillyPlugin> GetLoadedPlugins()
        => _loadedPlugins.AsReadOnly();

    /// <summary>
    /// Gets a specific plugin by ID.
    /// </summary>
    public IDarkLillyPlugin GetPluginById(string pluginId)
    {
        var plugin = _loadedPlugins.FirstOrDefault(p => p.PluginInfo.Id == pluginId);

        return plugin ?? throw new InvalidOperationException($"Plugin '{pluginId}' not found.");
    }

    /// <summary>
    /// Sorts plugins in dependency order (topological sort).
    /// </summary>
    public IEnumerable<PluginInfo> GetPluginsInDependencyOrder(IReadOnlyList<PluginInfo> allPlugins)
        => _dependencyValidator.TopologicalSort(allPlugins);

    public IReadOnlyList<string> GetScriptEngineLoadFunctions()
        => _scriptEngineLoadFunctions.AsReadOnly();

    /// <summary>
    /// Registers a plugin after validating its dependencies.
    /// </summary>
    public void RegisterPlugin(IDarkLillyPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        var pluginData = plugin.PluginInfo;

        if (_loadedPluginData.Any(p => p.Id == pluginData.Id))
        {
            throw new PluginLoadException(
                $"Plugin '{pluginData.Id}' is already loaded.",
                pluginData.Id,
                pluginData,
                GetLoadedPluginData()
            );
        }

        _dependencyValidator.ValidateDependencies(pluginData, GetLoadedPluginData());

        _loadedPlugins.Add(plugin);
        _loadedPluginData.Add(pluginData);

        var scriptFunction = plugin.GetScriptOnLoadFunctionName();

        if (!string.IsNullOrWhiteSpace(scriptFunction))
        {
            _scriptEngineLoadFunctions.Add(scriptFunction);
        }
    }
}
