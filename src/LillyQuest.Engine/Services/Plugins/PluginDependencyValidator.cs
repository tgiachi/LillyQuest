using LillyQuest.Core.Data.Plugins;
using LillyQuest.Engine.Exceptions.Plugins;

namespace LillyQuest.Engine.Services.Plugins;

/// <summary>
/// Validates plugin dependencies and detects circular dependency chains.
/// </summary>
public class PluginDependencyValidator
{
    /// <summary>
    /// Detects circular dependencies in a plugin graph.
    /// </summary>
    /// <param name="allPlugins">All plugins to check.</param>
    /// <returns>Empty list if no circular dependencies, otherwise list of plugin IDs in cycle.</returns>
    public List<string> DetectCircularDependencies(IReadOnlyList<PluginInfo> allPlugins)
    {
        var pluginMap = allPlugins.ToDictionary(p => p.Id, p => p);
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();

        foreach (var plugin in allPlugins)
        {
            if (!visited.Contains(plugin.Id))
            {
                var cycle = DetectCycleDFS(plugin.Id, pluginMap, visited, recursionStack);

                if (cycle.Count != 0)
                {
                    return cycle;
                }
            }
        }

        return new();
    }

    /// <summary>
    /// Sorts plugins in dependency order (topological sort).
    /// </summary>
    /// <param name="allPlugins">All plugins to sort.</param>
    /// <returns>Sorted list where dependencies come before dependents.</returns>
    public IEnumerable<PluginInfo> TopologicalSort(IReadOnlyList<PluginInfo> allPlugins)
    {
        var pluginMap = allPlugins.ToDictionary(p => p.Id, p => p);
        var visited = new HashSet<string>();
        var result = new List<PluginInfo>();

        foreach (var plugin in allPlugins)
        {
            if (!visited.Contains(plugin.Id))
            {
                TopologicalSortDFS(plugin.Id, pluginMap, visited, result);
            }
        }

        return result;
    }

    /// <summary>
    /// Validates that all declared dependencies for a plugin are loaded.
    /// </summary>
    /// <param name="pluginData">The plugin to validate.</param>
    /// <param name="loadedPlugins">List of already loaded plugins.</param>
    /// <returns>True if all dependencies are satisfied.</returns>
    /// <exception cref="PluginLoadException">If dependencies are missing.</exception>
    public bool ValidateDependencies(
        PluginInfo pluginData,
        IReadOnlyList<PluginInfo> loadedPlugins
    )
    {
        if (pluginData.Dependencies == null || pluginData.Dependencies.Length == 0)
        {
            return true;
        }

        var loadedPluginIds = loadedPlugins.Select(p => p.Id).ToHashSet();

        var missingDependencies = pluginData.Dependencies
                                            .Where(dep => !loadedPluginIds.Contains(dep))
                                            .ToList();

        if (missingDependencies.Count != 0)
        {
            throw new PluginLoadException(
                $"Plugin '{pluginData.Id}' has missing dependencies: {string.Join(", ", missingDependencies)}",
                pluginData.Id,
                pluginData,
                loadedPlugins
            );
        }

        return true;
    }

    private List<string> DetectCycleDFS(
        string nodeId,
        Dictionary<string, PluginInfo> pluginMap,
        HashSet<string> visited,
        HashSet<string> recursionStack,
        List<string>? path = null
    )
    {
        path ??= new();

        visited.Add(nodeId);
        recursionStack.Add(nodeId);
        path.Add(nodeId);

        if (!pluginMap.TryGetValue(nodeId, out var plugin))
        {
            recursionStack.Remove(nodeId);
            path.RemoveAt(path.Count - 1);

            return new();
        }

        foreach (var dependency in plugin.Dependencies ?? Array.Empty<string>())
        {
            if (!visited.Contains(dependency))
            {
                var cycle = DetectCycleDFS(dependency, pluginMap, visited, recursionStack, new(path));

                if (cycle.Count != 0)
                {
                    return cycle;
                }
            }
            else if (recursionStack.Contains(dependency))
            {
                var cycleStart = path.IndexOf(dependency);

                return path.Skip(cycleStart).ToList();
            }
        }

        recursionStack.Remove(nodeId);
        path.RemoveAt(path.Count - 1);

        return [];
    }

    private void TopologicalSortDFS(
        string nodeId,
        Dictionary<string, PluginInfo> pluginMap,
        HashSet<string> visited,
        List<PluginInfo> result
    )
    {
        visited.Add(nodeId);

        if (pluginMap.TryGetValue(nodeId, out var plugin))
        {
            foreach (var dependency in plugin.Dependencies ?? Array.Empty<string>())
            {
                if (!visited.Contains(dependency))
                {
                    TopologicalSortDFS(dependency, pluginMap, visited, result);
                }
            }

            result.Add(plugin);
        }
    }
}
