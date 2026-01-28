using DryIoc;
using LillyQuest.Core.Data.Directories;
using LillyQuest.Core.Data.Plugins;

namespace LillyQuest.Engine.Interfaces.Plugins;

/// <summary>
/// Defines the contract for engine plugins.
/// </summary>
public interface ILillyQuestPlugin
{
    /// <summary>
    /// Gets plugin metadata.
    /// </summary>
    PluginInfo PluginInfo { get; }

    /// <summary>
    /// Returns the bootstrap script function name to execute on load.
    /// </summary>
    string? GetScriptOnLoadFunctionName();

    /// <summary>
    /// Called when the engine is ready for plugin initialization.
    /// </summary>
    /// <param name="gameEntityManager">Game object manager for runtime access.</param>
    /// <param name="container">The DI container.</param>
    // void Ready(IGameEntityManager gameEntityManager, IContainer container);

    /// <summary>
    /// Registers plugin services in the container.
    /// </summary>
    /// <param name="container">The DI container.</param>
    void RegisterServices(IContainer container);

    /// <summary>
    ///  Returns a list of directories the plugin needs created.
    /// </summary>
    /// <returns></returns>
    string[]? DirectoriesToCreate();

    /// <summary>
    /// Provides directory configurations for global and plugin-specific roots.
    /// </summary>
    /// <param name="global">The global directories configuration.</param>
    /// <param name="plugin">The plugin-specific directories configuration.</param>
    void OnDirectories(DirectoriesConfig globalConfig, DirectoriesConfig plugin);

    /// <summary>
    /// Called when the engine is shutting down.
    /// </summary>
    void Shutdown();

    /// <summary>
    /// Called when the engine is fully initialized but before the window is visible.
    /// Use this for non-rendering setup tasks.
    /// </summary>
    /// <param name="container">The DI container.</param>
    async Task OnEngineReady(IContainer container)
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Called after the window is created and rendering is available.
    /// Use this for graphics resource setup.
    /// </summary>
    /// <param name="container">The DI container.</param>
    Task OnReadyToRender(IContainer container);

    /// <summary>
    /// Called when the bootstrap is loading resources. The LogScreen is displayed during this phase
    /// showing all historical logs. Plugins should log their loading progress here.
    /// </summary>
    /// <param name="container">The DI container.</param>
    Task OnLoadResources(IContainer container);
}
