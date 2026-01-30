using DryIoc;
using LillyQuest.Engine.Interfaces.Plugins;
using Serilog;

namespace LillyQuest.Engine.Bootstrap;

/// <summary>
/// Executes plugin lifecycle hooks in sequence with proper error handling.
/// </summary>
public sealed class PluginLifecycleExecutor
{
    private readonly ILogger _logger = Log.ForContext<PluginLifecycleExecutor>();
    private readonly IReadOnlyList<ILillyQuestPlugin> _plugins;

    public PluginLifecycleExecutor(IReadOnlyList<ILillyQuestPlugin> plugins)
        => _plugins = plugins;

    /// <summary>
    /// Executes OnEngineReady on all plugins in sequence.
    /// </summary>
    public async Task ExecuteOnEngineReady(IContainer container)
    {
        await ExecutePhase("OnEngineReady", async plugin => await plugin.OnEngineReady(container));
    }

    /// <summary>
    /// Executes OnLoadResources on all plugins in sequence.
    /// </summary>
    public async Task ExecuteOnLoadResources(IContainer container)
    {
        await ExecutePhase("OnLoadResources", async plugin => await plugin.OnLoadResources(container));
    }

    /// <summary>
    /// Executes OnReadyToRender on all plugins in sequence.
    /// </summary>
    public async Task ExecuteOnReadyToRender(IContainer container)
    {
        await ExecutePhase("OnReadyToRender", async plugin => await plugin.OnReadyToRender(container));
    }

    private async Task ExecutePhase(string phaseName, Func<ILillyQuestPlugin, Task> hookMethod)
    {
        foreach (var plugin in _plugins)
        {
            try
            {
                _logger.Information($"Executing {phaseName} for plugin {plugin.PluginInfo.Id}");
                await hookMethod(plugin);
                _logger.Information($"✓ {phaseName} completed for {plugin.PluginInfo.Id}");
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, $"✗ {phaseName} failed for plugin {plugin.PluginInfo.Id}");

                throw;
            }
        }
    }
}
