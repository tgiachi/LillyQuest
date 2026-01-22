using LillyQuest.Engine.Interfaces.Systems;
using LillyQuest.Engine.Types;

namespace LillyQuest.Engine.Interfaces.Services;

/// <summary>
/// Manages system registration and tracks execution metrics by query type.
/// </summary>
public interface ISystemManager
{
    /// <summary>
    /// Gets the last recorded processing time for a query type.
    /// </summary>
    TimeSpan GetSystemProcessingTime(SystemQueryType queryType);

    /// <summary>
    /// Initializes all registered systems.
    /// </summary>
    void InitializeAllSystems();

    /// <summary>
    /// Registers a system so it can run for its declared query types.
    /// </summary>
    void RegisterSystem<TSystem>(TSystem system) where TSystem : ISystem;

    /// <summary>
    /// Unregisters a system from all its declared query types.
    /// </summary>
    void RemoveSystem<TSystem>(TSystem system) where TSystem : ISystem;
}
