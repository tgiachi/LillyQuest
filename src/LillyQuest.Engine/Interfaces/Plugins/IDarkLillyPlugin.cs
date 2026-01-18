using DryIoc;
using LillyQuest.Core.Data.Plugins;
using LillyQuest.Engine.Interfaces.Managers;

namespace LillyQuest.Engine.Interfaces.Plugins;

/// <summary>
/// Defines the contract for engine plugins.
/// </summary>
public interface IDarkLillyPlugin
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
    void Ready(IGameEntityManager gameEntityManager, IContainer container);

    /// <summary>
    /// Registers plugin services in the container.
    /// </summary>
    /// <param name="container">The DI container.</param>
    void RegisterServices(IContainer container);

    /// <summary>
    /// Called when the engine is shutting down.
    /// </summary>
    void Shutdown();
}
