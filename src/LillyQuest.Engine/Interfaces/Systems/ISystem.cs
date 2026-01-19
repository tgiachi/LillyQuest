namespace LillyQuest.Engine.Interfaces.Systems;

/// <summary>
/// Base interface for all engine systems.
/// Systems are responsible for managing specific aspects of the engine (rendering, physics, input, etc).
/// </summary>
public interface ISystem
{
    /// <summary>
    /// Human-readable name of the system.
    /// Used for logging and debugging.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Execution priority of the system.
    /// Lower values execute first. Used to control the order of system execution.
    /// </summary>
    uint Priority { get; }

    /// <summary>
    /// Called when the system is first registered with the SystemManager.
    /// Use this to initialize resources, dependencies, and state.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Called when the system is unregistered or the engine is shutting down.
    /// Use this to clean up resources and perform cleanup operations.
    /// </summary>
    void Shutdown();
}
